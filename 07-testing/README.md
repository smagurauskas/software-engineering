# Testing demo: word game

Demo for the [Testing lecture](../07-testing.qmd). Requires .NET 10 SDK and Node.js 22+.

```bash
cd WordGame.Web && npm install && npm run build && cd ..
dotnet run --project WordGame.Api
dotnet test
```

Run one test type with `dotnet test --project WordGame.UnitTests` (or `IntegrationTests`, `E2ETests`).
Set `HEADED=1` to watch the E2E tests in a browser.
