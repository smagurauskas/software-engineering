using WordGame.Api;
using WordGame.Construction.Complexity;

namespace WordGame.Construction.Tests;

// #region characterization
// Characterization tests: written against the old code before refactoring it, they pin down
// what it does today. All three versions must give the same result for every case, so the
// refactoring is known to have changed the structure only, not the behavior.
public class ExperienceCalculatorTests
{
    private static readonly DateTimeOffset Monday = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Saturday = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("", 2, 0, 0, false, false, false, 0)]                         // no words
    [InlineData("hello world game", 2, 0, 0, false, false, false, 30)]        // 10 per word
    [InlineData("programming language game", 2, 0, 0, false, false, false, 35)] // longer than 8 letters: 15
    [InlineData("hello world game", 0.5, 0, 0, false, false, false, 45)]      // fast: +5 per word
    [InlineData("hello world game", 1, 0, 0, false, false, false, 30)]        // exactly a minute is not fast
    [InlineData("hello world game", 0.5, 1, 0, false, false, false, 25)]      // a hint costs 5, no speed bonus
    [InlineData("hello world game", 2, 2, 0, false, false, false, 20)]
    [InlineData("hello world game", 2, 3, 0, false, false, false, 15)]        // a hint for every word halves
    [InlineData("hello world game", 2, 0, 0, false, false, true, 60)]         // weekend doubles
    [InlineData("hello world game", 0.5, 0, 0, false, false, true, 90)]       // ...including the speed bonus
    [InlineData("hello world game", 2, 0, 2, false, false, false, 30)]        // streak shorter than 3 days
    [InlineData("hello world game", 2, 0, 3, false, false, false, 50)]
    [InlineData("hello world game", 2, 0, 6, false, false, false, 50)]
    [InlineData("hello world game", 2, 0, 7, false, false, false, 80)]
    [InlineData("hello world game", 2, 0, 0, true, false, false, 50)]         // first game of the day
    [InlineData("hello world game", 2, 0, 0, false, true, false, 45)]         // premium: x1.5
    [InlineData("hello world game", 0.5, 1, 0, false, true, false, 37)]       // ...rounded down
    [InlineData("hello world game", 0.5, 0, 7, true, true, true, 200)]        // capped at 200
    public void AllVersions_AwardTheSameExperience(
        string words, double minutes, int hints, int streak, bool first, bool premium, bool weekend, int expected)
    {
        var startedAt = weekend ? Saturday : Monday;
        var game = new Game("player@test.com", words.Split(' ', StringSplitOptions.RemoveEmptyEntries), startedAt);
        var completed = new CompletedGame(game, startedAt.AddMinutes(minutes), hints, streak, first, premium);

        Assert.Equal(expected, ExperienceCalculatorBefore.Calculate(completed));
        Assert.Equal(expected, ExperienceCalculator.Calculate(completed));
        Assert.Equal(expected, ExperienceCalculatorOverSplit.Calculate(completed));
    }
}
// #endregion characterization
