"""Finds hotspots and change coupling in a git repository.

Hotspots are files that are both complex and changed often: the places where
improving the code pays off most. Change coupling lists files that are changed
in the same commits, which often reveals dependencies the code does not show.

Complexity is approximated by indentation (sum of indentation levels of the
non-blank lines), which works for any language and correlates well with
cyclomatic complexity. See "Your Code as a Crime Scene" by Adam Tornhill.

    python hotspots.py <repo> [--rev REV] [--include GLOB ...] [--exclude GLOB ...] [--top N]

Run it on your own project, e.g. `python hotspots.py . --include "*.cs"`.
"""

import argparse
import fnmatch
import itertools
import subprocess
from collections import Counter

# Commits touching more files than this are usually mass changes (renames,
# formatting, migrations) that say nothing about how the code is coupled.
MAX_FILES_PER_COMMIT = 20


def git(repo, *args):
    return subprocess.run(
        ["git", "-C", repo, *args], capture_output=True, text=True, encoding="utf-8", check=True
    ).stdout


def commits(repo, rev):
    """Yields the set of files changed in every commit reachable from rev."""
    log = git(repo, "log", rev, "--no-merges", "--format=--%H", "--name-only")
    for chunk in log.split("--")[1:]:
        _, *files = chunk.strip().splitlines()
        yield {f for f in files if f}


def indentation_complexity(text):
    total = 0
    for line in text.expandtabs(4).splitlines():
        if line.strip():
            total += (len(line) - len(line.lstrip())) // 4
    return total


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("repo")
    parser.add_argument("--rev", default="HEAD", help="analyze history up to this commit")
    parser.add_argument("--include", nargs="+", default=["*"], help="file name patterns to analyze")
    parser.add_argument("--exclude", nargs="+", default=[], help="patterns to skip, e.g. generated code")
    parser.add_argument("--top", type=int, default=10)
    args = parser.parse_args()

    existing = set(git(args.repo, "ls-tree", "-r", "--name-only", args.rev).splitlines())

    def included(path):
        return (
            path in existing
            and any(fnmatch.fnmatch(path, p) for p in args.include)
            and not any(fnmatch.fnmatch(path, p) for p in args.exclude)
        )

    revisions = Counter()
    shared = Counter()
    for files in commits(args.repo, args.rev):
        files = sorted(f for f in files if included(f))
        revisions.update(files)
        if len(files) <= MAX_FILES_PER_COMMIT:
            shared.update(itertools.combinations(files, 2))

    rows = []
    for path, changes in revisions.items():
        text = git(args.repo, "cat-file", "blob", f"{args.rev}:{path}")
        lines = sum(1 for line in text.splitlines() if line.strip())
        rows.append((changes * indentation_complexity(text), changes, lines, indentation_complexity(text), path))

    print(f"Hotspots (top {args.top})")
    print(f"{'Changes':>7} | {'Lines':>5} | {'Complexity':>10} | File")
    for _, changes, lines, complexity, path in sorted(rows, reverse=True)[: args.top]:
        print(f"{changes:>7} | {lines:>5} | {complexity:>10} | {path}")

    print()
    print(f"Change coupling (top {args.top}, at least 3 shared commits)")
    print(f"{'Shared':>6} | {'Degree':>6} | Files")
    coupled = []
    for (a, b), count in shared.items():
        if count >= 3:
            # Shared commits relative to the average number of commits of the two files
            degree = count / ((revisions[a] + revisions[b]) / 2)
            coupled.append((degree, count, a, b))
    for degree, count, a, b in sorted(coupled, reverse=True)[: args.top]:
        print(f"{count:>6} | {degree:>6.0%} | {a}  <->  {b}")


if __name__ == "__main__":
    main()
