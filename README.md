# Software Engineering

Software Engineering lecture material.

Material is in Jupyter Notebook format `ipynb`, because it allows to blend formatted code with the executable code. `ipynb` itself is a `json` format document, which is best read in an editor with dedicated support. Recommendation is to use VS Code with a few extensions as described in the section.

## Notebook usage

Suggested approach is to use [VS Code](https://code.visualstudio.com/) with the following extensions:
- [Jupyter](https://marketplace.visualstudio.com/items?itemName=ms-toolsai.jupyter)
- [Polyglot Notebooks](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
- [Markdown Preview Mermaid Support](https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid)

## Contributing

Pull requests are welcome. Few things to keep in mind when contributing:
- Do not change the general lecture structure like order of the lectures or top-level parts of the notebooks.
- Text should be in passive tense where possible.
- Use mermaid diagrams instead of image diagrams if possible.
- It should be possible to use notebooks as slides using `nbconvert`. It relies on correct slide type assigned to each cell as well as content in cell being *relatively* small. If the cell is too big to be displayed as a slide on screen, then consider splitting it into smaller cells instead.
- Do not commit cell output.
- Prefer open source examples and tools where possible.

## Launching the slides:
`jupyter nbconvert <notebook file name> --to slides --post serve --no-input --no-prompt`
