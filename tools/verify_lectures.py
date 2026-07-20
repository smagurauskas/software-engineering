"""Verify that the C# code in the lecture .qmd files still works.

Each lecture declares a verification tier in its frontmatter:

    verify: output   - code is executed and stdout must match expected/<name>.txt
    verify: run      - code is executed; only "no compile errors" is asserted
                       (used for lectures with nondeterministic output)
    verify: none     - code is not executed (needs external services, packages,
                       very long runtime, or has no code at all)

An `output` lecture with a few genuinely volatile cells (e.g. printing live
memory usage) can list them as `verify-volatile-cells: [87]`; those cells'
output is replaced with a placeholder before comparison, so the rest of the
lecture stays snapshot-verified. Random scripting assembly names (ℛ*<guid>)
are normalized away automatically.

Execution uses tools/LectureRunner, which replays cells as chained Roslyn
scripting submissions - the same semantics the original notebooks had.

Usage:
    python tools/verify_lectures.py             # verify everything
    python tools/verify_lectures.py 06-linq.qmd # verify specific lectures
    python tools/verify_lectures.py --update    # re-record expected outputs
"""

import glob
import pathlib
import re
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
RUNNER = ROOT / "tools" / "LectureRunner"
EXPECTED_DIR = ROOT / "expected"
ACTUAL_DIR = ROOT / ".verify"
TIMEOUT_SECONDS = 600


def frontmatter(path):
    text = path.read_text(encoding="utf-8")
    match = re.match(r"\A---\n(.*?)\n---\n", text, re.S)
    block = match.group(1) if match else ""
    tier = re.search(r"^verify:\s*(\S+)", block, re.M)
    volatile = re.search(r"^verify-volatile-cells:\s*\[([\d,\s]*)\]", block, re.M)
    cells = ({int(n) for n in volatile.group(1).split(",") if n.strip()}
             if volatile else set())
    return (tier.group(1) if tier else "none"), cells


CELL_MARKER = re.compile(r"^── cell (\d+) ──$")
# Roslyn scripting compiles each run into randomly named assemblies; lecture
# code that prints type/assembly info would leak that randomness.
SCRIPT_ASSEMBLY = re.compile(r"ℛ\*[0-9a-f-]+#[0-9-]+")


def normalize(text, volatile_cells=frozenset()):
    lines = [line.rstrip() for line in text.replace("\r\n", "\n").split("\n")]
    out, masking, masked = [], False, False
    for line in lines:
        marker = CELL_MARKER.match(line)
        if marker:
            masking = int(marker.group(1)) in volatile_cells
            masked = False
            out.append(line)
            continue
        if masking:
            if not masked:
                out.append("<volatile output masked>")
                masked = True
            continue
        out.append(SCRIPT_ASSEMBLY.sub("ℛ*<dynamic>", line))
    return "\n".join(out).strip() + "\n"


def run_lecture(qmd, volatile_cells):
    result = subprocess.run(
        ["dotnet", "run", "--project", str(RUNNER), "-c", "Release",
         "--no-build", "--", str(qmd)],
        capture_output=True, text=True, encoding="utf-8",
        cwd=ROOT, timeout=TIMEOUT_SECONDS,
    )
    return result.returncode, normalize(result.stdout, volatile_cells)


def main():
    update = "--update" in sys.argv
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    qmds = [ROOT / a for a in args] or sorted(ROOT.glob("*.qmd"))

    print("Building LectureRunner...")
    build = subprocess.run(
        ["dotnet", "build", str(RUNNER), "-c", "Release", "--nologo", "-v", "q"],
        cwd=ROOT,
    )
    if build.returncode != 0:
        sys.exit("LectureRunner build failed")

    ACTUAL_DIR.mkdir(exist_ok=True)
    failures = []
    summary = []

    for qmd in qmds:
        tier, volatile_cells = frontmatter(qmd)
        name = qmd.stem
        if tier == "none":
            summary.append(f"  skip    {name}")
            continue

        code, actual = run_lecture(qmd, volatile_cells)
        (ACTUAL_DIR / f"{name}.txt").write_text(actual, encoding="utf-8", newline="\n")

        if code != 0:
            summary.append(f"  FAIL    {name} (compile error, exit {code})")
            failures.append(name)
            continue

        if tier == "run":
            summary.append(f"  ran     {name}")
            continue

        # tier == "output"
        expected_path = EXPECTED_DIR / f"{name}.txt"
        if update:
            EXPECTED_DIR.mkdir(exist_ok=True)
            expected_path.write_text(actual, encoding="utf-8", newline="\n")
            summary.append(f"  updated {name}")
            continue

        if not expected_path.exists():
            summary.append(f"  FAIL    {name} (missing {expected_path.name}; "
                           f"run with --update)")
            failures.append(name)
            continue

        expected = normalize(expected_path.read_text(encoding="utf-8"))
        if actual != expected:
            summary.append(f"  FAIL    {name} (output differs; compare "
                           f"expected/{name}.txt with .verify/{name}.txt)")
            failures.append(name)
        else:
            summary.append(f"  ok      {name}")

    print("\n".join(summary))
    if failures:
        print(f"\n{len(failures)} lecture(s) failed verification.")
        sys.exit(1)
    print("\nAll verified lectures passed.")


if __name__ == "__main__":
    main()
