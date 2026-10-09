using System.Net;
using System.Net.Http.Json;
using WordGame.Api;
using WordGame.Testing;

namespace WordGame.IntegrationTests;

// #region setup
// IClassFixture: one application is started for all tests in this class,
// because starting it for every test would be slow.
public class GamesApiTests(WordGameFactory factory) : IClassFixture<WordGameFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // The application is shared, so every test plays as its own player
    // to avoid seeing data left behind by the other tests.
    private readonly string _email = $"{Guid.NewGuid()}@test.com";

    private CancellationToken Cancellation => TestContext.Current.CancellationToken;
    // #endregion setup

    // #region full-flow
    [Fact]
    public async Task CompleteGame_CorrectTranslations_ExperienceIsSavedForUser()
    {
        await _client.PostAsJsonAsync("/api/games", new StartGameRequest(_email), Cancellation);

        var completion = await _client.PostAsJsonAsync(
            "/api/games/complete",
            new CompleteGameRequest(_email, ["hallo", "welt", "spiel"]),
            Cancellation);
        var user = await _client.GetFromJsonAsync<User>($"/api/users/{_email}", Cancellation);

        Assert.Equal(HttpStatusCode.OK, completion.StatusCode);
        Assert.Equal(30, user!.Experience);
    }
    // #endregion full-flow

    [Fact]
    public async Task StartGame_ReturnsWordsAsJson()
    {
        var response = await _client.PostAsJsonAsync("/api/games", new StartGameRequest(_email), Cancellation);
        var json = await response.Content.ReadAsStringAsync(Cancellation);

        // Checks the actual JSON contract the frontend relies on, including property name casing
        Assert.Equal("""{"words":["hello","world","game"]}""", json);
    }

    [Fact]
    public async Task GetUser_UnknownEmail_Returns404()
    {
        var response = await _client.GetAsync($"/api/users/{_email}", Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompleteGame_SubmittedTwice_AwardsExperienceOnce()
    {
        await _client.PostAsJsonAsync("/api/games", new StartGameRequest(_email), Cancellation);
        var request = new CompleteGameRequest(_email, ["hallo", "welt", "spiel"]);

        await _client.PostAsJsonAsync("/api/games/complete", request, Cancellation);
        var second = await _client.PostAsJsonAsync("/api/games/complete", request, Cancellation);
        var secondResult = await second.Content.ReadFromJsonAsync<CompleteGameResponse>(Cancellation);
        var user = await _client.GetFromJsonAsync<User>($"/api/users/{_email}", Cancellation);

        Assert.False(secondResult!.Success);
        Assert.Equal(30, user!.Experience);
    }
}
