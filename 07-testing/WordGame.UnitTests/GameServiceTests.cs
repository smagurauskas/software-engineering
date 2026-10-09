using Microsoft.Extensions.Time.Testing;
using WordGame.Api;
using WordGame.Testing;

namespace WordGame.UnitTests;

// #region setup
public class GameServiceTests
{
    // A Monday, so the weekend bonus does not apply unless a test moves the clock
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeUserRepository _users = new();
    private readonly GameService _gameService;

    // xUnit creates a new instance of the class for every test, so every test gets fresh dependencies
    public GameServiceTests()
    {
        // The "unit" here is GameService together with UserService and InMemoryGameStore:
        // they are fast and deterministic, so there is no reason to replace them.
        // Only the file, the randomness and the clock are replaced.
        _gameService = new GameService(
            new FixedWordPicker("hello", "world", "game"),
            new InMemoryGameStore(),
            new UserService(_users),
            _time);
    }
    // #endregion setup

    [Fact]
    public void StartGame_ReturnsWordsToTranslate()
    {
        var response = _gameService.StartGame("player@test.com");

        Assert.Equal(["hello", "world", "game"], response.Words);
    }

    // #region correct-translations
    [Fact]
    public void CompleteGame_CorrectTranslations_AwardsExperience()
    {
        // Arrange
        _gameService.StartGame("player@test.com");

        // Act
        var response = _gameService.CompleteGame("player@test.com", ["hallo", "welt", "spiel"]);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(30, response.ExperienceGained);
        Assert.Equal(30, _users.FindByEmail("player@test.com")!.Experience);
    }
    // #endregion correct-translations

    [Fact]
    public void CompleteGame_WrongTranslation_AwardsNoExperience()
    {
        _gameService.StartGame("player@test.com");

        var response = _gameService.CompleteGame("player@test.com", ["hallo", "welt", "wrong"]);

        Assert.False(response.Success);
        Assert.Null(_users.FindByEmail("player@test.com"));
    }

    // #region theory
    [Theory]
    [InlineData("HALLO", "WELT", "SPIEL")]
    [InlineData("Hallo", "Welt", "Spiel")]
    [InlineData(" hallo", "welt ", " spiel ")]
    public void CompleteGame_TranslationsWithDifferentCaseOrSpaces_AreAccepted(
        string first, string second, string third)
    {
        _gameService.StartGame("player@test.com");

        var response = _gameService.CompleteGame("player@test.com", [first, second, third]);

        Assert.True(response.Success);
    }
    // #endregion theory

    [Fact]
    public void CompleteGame_WithoutStartedGame_Fails()
    {
        var response = _gameService.CompleteGame("player@test.com", ["hallo", "welt", "spiel"]);

        Assert.False(response.Success);
    }

    [Fact]
    public void CompleteGame_SubmittedTwice_AwardsExperienceOnce()
    {
        _gameService.StartGame("player@test.com");

        _gameService.CompleteGame("player@test.com", ["hallo", "welt", "spiel"]);
        var secondResponse = _gameService.CompleteGame("player@test.com", ["hallo", "welt", "spiel"]);

        Assert.False(secondResponse.Success);
        Assert.Equal(30, _users.FindByEmail("player@test.com")!.Experience);
    }

    // #region time
    [Fact]
    public void CompleteGame_AfterTimeLimit_Fails()
    {
        _gameService.StartGame("player@test.com");

        // No waiting: the fake clock is moved forward instantly
        _time.Advance(TimeSpan.FromMinutes(6));
        var response = _gameService.CompleteGame("player@test.com", ["hallo", "welt", "spiel"]);

        Assert.False(response.Success);
    }

    [Fact]
    public void CompleteGame_OnWeekend_AwardsDoubleExperience()
    {
        _time.SetUtcNow(new DateTimeOffset(2026, 10, 10, 12, 0, 0, TimeSpan.Zero)); // Saturday
        _gameService.StartGame("player@test.com");

        var response = _gameService.CompleteGame("player@test.com", ["hallo", "welt", "spiel"]);

        Assert.Equal(60, response.ExperienceGained);
    }
    // #endregion time
}
