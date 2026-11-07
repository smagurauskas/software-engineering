using WordGame.Api;
using WordGame.Construction.Complexity;

namespace WordGame.Construction.WeakTests;

// #region weak
public class ExperienceCalculatorWeakTests
{
    private static readonly DateTimeOffset Saturday = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    // Every rule is executed, so every line runs. But the assertion would still
    // pass if any of the rules awarded a different amount, or no amount at all.
    [Theory]
    [InlineData(0.5, 0, 7, true, true)]
    [InlineData(2, 1, 3, false, false)]
    [InlineData(2, 3, 0, false, false)]
    public void Calculate_AwardsSomeExperience(double minutes, int hints, int streak, bool first, bool premium)
    {
        var game = new Game("player@test.com", ["programming", "world", "game"], Saturday);
        var completed = new CompletedGame(game, Saturday.AddMinutes(minutes), hints, streak, first, premium);

        var experience = ExperienceCalculator.Calculate(completed);

        Assert.InRange(experience, 1, ExperienceCalculator.MaxExperience);
    }

    [Fact]
    public void Calculate_NoWords_AwardsNothing()
    {
        var game = new Game("player@test.com", [], Saturday);

        Assert.Equal(0, ExperienceCalculator.Calculate(new CompletedGame(game, Saturday, 0, 0, false, false)));
    }
}
// #endregion weak
