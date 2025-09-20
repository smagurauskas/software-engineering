# Software Engineering

Software Engineering lecture material.

Material is based on Polyglot notebooks is in Jupyter Notebook format `ipynb`. It allows to blend formatted text with the executable code. `ipynb` itself is a `json` format document, which is best read in an editor with dedicated support. Recommendation is to use VS Code with a few extensions as described in the section. Read more about this here https://github.com/dotnet/interactive/blob/main/docs/FAQ.md.

## Notebook usage

Suggested approach is to use [VS Code](https://code.visualstudio.com/) with the following extensions:

- [Jupyter](https://marketplace.visualstudio.com/items?itemName=ms-toolsai.jupyter)
- [Polyglot Notebooks](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
- [Markdown Preview Mermaid Support](https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid)

## Contributing

If you would like to contribute but unsure on what exactly: take a look at Issues section on GitHub - <https://github.com/smagurauskas/software-engineering/issues>. Here are the currently relevant issues are tracked. If you would like to start working one, then post about it in the issue comments.

Pull requests are welcome. Few things to keep in mind when contributing:

- Do not change the general lecture structure like order of the lectures or top-level parts of the notebooks. Try to keep the general course structure as it is now, and prioritize contributing to existing notebooks instead.
- Text should be in passive tense where possible.
- Use mermaid diagrams instead of image diagrams if possible.
- It should be possible to use notebooks as slides using `nbconvert`. It relies on correct slide type assigned to each cell as well as content in cell being _relatively_ small. If the cell is too big to be displayed as a slide on screen, then consider splitting it into smaller cells instead.
- Do not commit cell output.
- Prefer open source examples and tools where possible.

## Launching the slides

To launch the notebook in slide mode use this command:

```bash
jupyter nbconvert <slide file> --to slides --post serve --template reveal
```

However this relies that all the cells would have proper slide type assigned to them. If slide does not have a slide type assigned, then whole notebook conversion might fail in interesting ways.

It is known that mermaid diagrams not always convert to slides properly on the first go. Refreshing the browser at location of the slide for few times usually helps.

## Not committing output

Add the following filter to git config:

```bash
git config filter.strip-notebook-output.clean 'jupyter nbconvert --ClearOutputPreprocessor.enabled=True --to=notebook --stdin --stdout --log-level=ERROR'
```
