# Software Engineering

Software Engineering lecture material.

The material is written in [Quarto](https://quarto.org) (`.qmd` files — plain
markdown with a small amount of front matter).

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
quarto preview 06-linq.qmd --to revealjs
```

## Verifying lecture code

Record the expected output (after intentionally changing code output):

```bash
python tools/verify_lectures.py --update
```

Verify that the code output matches the recording:

```bash
python tools/verify_lectures.py
```

## Writing conventions

Code fences written as ` ```csharp ` are never executed; executable cells use
` ```{.csharp} `. Keep executable cells small enough to fit a slide; prefer
several small cells over one large one.

- Text should be in passive tense where possible.
- Use mermaid diagrams (` ```{mermaid} ` blocks) instead of image
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
