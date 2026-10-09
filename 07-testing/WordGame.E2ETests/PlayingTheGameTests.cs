using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace WordGame.E2ETests;

// #region setup
public class PlayingTheGameTests(BrowserFixture fixture) : IClassFixture<BrowserFixture>, IAsyncLifetime
{
    private readonly string _email = $"{Guid.NewGuid()}@test.com";
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    // Every test gets a fresh browser context: no cookies or storage carried over
    public async ValueTask InitializeAsync()
    {
        _context = await fixture.Browser.NewContextAsync(new() { BaseURL = fixture.BaseUrl });
        await _context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true });
        _page = await _context.NewPageAsync();
    }

    // The trace records every step with screenshots, open it at https://trace.playwright.dev
    public async ValueTask DisposeAsync()
    {
        var test = TestContext.Current.TestMethod!.MethodName;
        var trace = Path.Combine(AppContext.BaseDirectory, "traces", $"{test}.zip");
        await _context.Tracing.StopAsync(new() { Path = trace });
        TestContext.Current.TestOutputHelper?.WriteLine($"Trace: {trace}");
        await _context.DisposeAsync();
    }
    // #endregion setup

    // #region correct
    [Fact]
    public async Task Player_TranslatesAllWordsCorrectly_SeesExperienceGained()
    {
        await _page.GotoAsync("/");
        await _page.GetByLabel("Email").FillAsync(_email);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Start game" }).ClickAsync();

        await _page.GetByLabel("hello", new() { Exact = true }).FillAsync("hallo");
        await _page.GetByLabel("world", new() { Exact = true }).FillAsync("welt");
        await _page.GetByLabel("game", new() { Exact = true }).FillAsync("spiel");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

        await Expect(_page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Correct! +30 XP");
        await Expect(_page.GetByText("Total experience: 30 XP")).ToBeVisibleAsync();
    }
    // #endregion correct

    [Fact]
    public async Task Player_MakesAMistake_GetsNoExperience()
    {
        await _page.GotoAsync("/");
        await _page.GetByLabel("Email").FillAsync(_email);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Start game" }).ClickAsync();

        await _page.GetByLabel("hello", new() { Exact = true }).FillAsync("hallo");
        await _page.GetByLabel("world", new() { Exact = true }).FillAsync("wrong");
        await _page.GetByLabel("game", new() { Exact = true }).FillAsync("spiel");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

        await Expect(_page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Some translations are wrong, try again");
        await Expect(_page.GetByText("Total experience: 0 XP")).ToBeVisibleAsync();
    }
}
