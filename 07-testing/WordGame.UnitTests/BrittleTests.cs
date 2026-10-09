using WordGame.Api;
using WordGame.Testing;

namespace WordGame.UnitTests;

// #region brittle
public class BrittleTests
{
    // Uses the real clock: passes Monday to Friday, fails on Saturday and Sunday,
    // because games completed on a weekend award double experience.
    [Fact(Skip = "Brittle on purpose: depends on the current day of the week")]
    public void CompleteGame_CorrectTranslations_Awards30Experience()
    {
        var gameService = new GameService(
            new FixedWordPicker("hello", "world", "game"),
            new InMemoryGameStore(),
            new UserService(new FakeUserRepository()),
            TimeProvider.System);
        gameService.StartGame("player@test.com");

        var response = gameService.CompleteGame("player@test.com", ["hallo", "welt", "spiel"]);

        Assert.Equal(30, response.ExperienceGained);
    }
}
// #endregion brittle
