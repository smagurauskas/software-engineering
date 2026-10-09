using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using WordGame.Testing;

namespace WordGame.E2ETests;

// #region fixture
// Shared by all E2E tests: the application on a real port and a browser.
public class BrowserFixture : IAsyncLifetime
{
    public WordGameFactory App { get; } = new();
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;

    private IPlaywright _playwright = null!;

    public async ValueTask InitializeAsync()
    {
        // The same test setup as the integration tests, but on a real Kestrel server (.NET 10+),
        // because a browser cannot talk to the in-memory test server.
        App.UseKestrel(0);
        App.StartServer();
        BaseUrl = App.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.First();
        EnsureFrontendIsBuilt();

        // Downloads the browser on the first run and does nothing afterwards,
        // so the tests still start with a single command
        Microsoft.Playwright.Program.Main(["install", "chromium"]);

        // HEADED=1 shows the browser and slows it down, so the audience can follow along
        var headed = Environment.GetEnvironmentVariable("HEADED") == "1";
        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new() { Headless = !headed, SlowMo = headed ? 700 : 0 });
    }
    // #endregion fixture

    private void EnsureFrontendIsBuilt()
    {
        var webRoot = App.Services.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        if (!File.Exists(Path.Combine(webRoot ?? "", "index.html")))
        {
            throw new InvalidOperationException(
                "The frontend is not built. Run `npm install` and `npm run build` in WordGame.Web first.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright.Dispose();
        await App.DisposeAsync();
    }
}
