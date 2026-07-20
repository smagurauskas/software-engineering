# Software Engineering

Software Engineering lecture material.

The material is written in [Quarto](https://quarto.org) (`.qmd` files — plain
markdown with a small amount of front matter). Every lecture renders from the
same source into two forms:

- `<lecture>.html` — a readable course-notes page
- `<lecture>.slides.html` — a reveal.js slide deck used during the lecture

The C# code inside the lectures is executable and is verified by CI on every
change, so code samples cannot silently rot (see *How code verification works*
below).

> The material previously lived in Polyglot Notebooks (`.ipynb`). That stack
> [was deprecated by Microsoft in 2026](https://github.com/dotnet/interactive/issues/4163),
> which prompted the move to Quarto.

## Prerequisites

- [Quarto CLI](https://quarto.org/docs/get-started/) — renders pages and slides
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — runs the lecture code
- Python 3 — runs the verification script (no packages needed)

## Working with the material

Live-reloading preview of a lecture page while editing:

```bash
quarto preview 06-linq.qmd
```

Render everything (pages + slides for all lectures, output in `_site/`):

```bash
quarto render
```

Render one lecture's slide deck only:

```bash
quarto render 06-linq.qmd --to revealjs
```

Slides follow reveal.js conventions: `##` headings start a new slide, press `S`
in the deck for speaker notes. Rendered output is never committed — `_site/`
and `*.html` are git-ignored; CI builds and publishes the site.

## Running the lecture code

Lecture code blocks are real, executable C#. To replay a whole lecture's code
the way the old notebooks executed it (cell by cell, state carried across
cells):

```bash
dotnet run --project tools/LectureRunner -- 06-linq.qmd
```

## How code verification works

Every lecture declares a tier in its front matter:

| Tier | Meaning |
|------|---------|
| `verify: output` | CI runs the lecture's code and its stdout must match `expected/<lecture>.txt` exactly |
| `verify: run` | CI runs the code and only asserts there are no compile errors (output is nondeterministic — threads, time, randomness) |
| `verify: none` | Code is not executed by CI (needs external services/NuGet packages, very long runtime, or the lecture has no code) |

The pieces:

- `tools/LectureRunner/` — a small .NET tool that extracts the ` ```{.csharp} `
  blocks from a `.qmd` and executes them as chained Roslyn scripting
  submissions, replicating notebook semantics: later cells see earlier state,
  may shadow variables, and a cell that throws prints the exception and
  execution continues. Compile errors mean the material has rotted and fail
  the run.
- `expected/*.txt` — committed "golden" outputs for `verify: output` lectures.
  This is snapshot testing: if you change lecture code and its output changes,
  CI fails until you re-record the snapshot, and the snapshot diff shows up in
  your pull request for review.
- `tools/verify_lectures.py` — the orchestrator that CI runs.

Verify locally:

```bash
python tools/verify_lectures.py                # everything
python tools/verify_lectures.py 06-linq.qmd    # one lecture
```

After intentionally changing code output, re-record the snapshots and commit
the diff:

```bash
python tools/verify_lectures.py --update
```

When adding a new lecture, pick the strictest tier the content allows:
`output` if two consecutive runs produce identical stdout, `run` if the code
is sound but output varies, `none` only when the code cannot run standalone.
If only a few cells are volatile (e.g. printing live memory usage), keep the
`output` tier and list them as `verify-volatile-cells: [87]` — their output is
masked before comparison while the rest of the lecture stays snapshot-verified.

## Writing conventions

Markdown-authored code fences (```` ```csharp ````) are illustrative only and
are never executed. Executable cells use the attribute form
(```` ```{.csharp} ````) — the verifier picks up exactly those. Keep executable
cells small enough to fit a slide; prefer several small cells over one large
one.

- Text should be in passive tense where possible.
- Use mermaid diagrams (```` ```{mermaid} ```` blocks) instead of image
  diagrams where possible.
- Prefer open source examples and tools where possible.

## Contributing

If you would like to contribute but are unsure on what exactly: take a look at
the Issues section on GitHub —
<https://github.com/smagurauskas/software-engineering/issues>. If you would
like to start working on one, then post about it in the issue comments.

Pull requests are welcome. Few things to keep in mind when contributing:

- Do not change the general lecture structure like the order of the lectures
  or top-level parts of the material. Prioritize contributing to existing
  lectures instead.
- CI must stay green: the site must render and code verification must pass.
  If your change legitimately alters code output, include the re-recorded
  `expected/*.txt` in the pull request.
