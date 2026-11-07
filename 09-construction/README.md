# Software construction demo: word game, extended

Demo for the [Software Construction lecture](../09-software-construction.qmd). Builds on the
word game of the [testing lecture](../07-testing) (`07-testing/WordGame.Api` is referenced as is).
Requires .NET 10 SDK and Python 3.

```bash
dotnet build          # the analyzers report the complexity of the *Before examples
dotnet test
dotnet tool restore   # Stryker.NET, for mutation testing
python measure.py     # re-measure everything the lecture quotes into reports/
```

Find the hotspots of any git repository:

```bash
python hotspots.py path/to/repo --include "*.cs" --exclude "*/Migrations/*"
```
