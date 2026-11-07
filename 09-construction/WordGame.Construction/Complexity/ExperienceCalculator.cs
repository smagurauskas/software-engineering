namespace WordGame.Construction.Complexity;

// #region after
// The same rules, refactored: one method states the order in which the rules
// apply, and every rule has a name. The rules themselves are not simpler -
// that complexity is essential, it comes from the requirements.
public static class ExperienceCalculator
{
    public const int MaxExperience = 200;
    private static readonly TimeSpan FastGame = TimeSpan.FromMinutes(1);

    public static int Calculate(CompletedGame completed)
    {
        if (completed.Game.Words.Length == 0)
        {
            return 0;
        }

        var experience = WordExperience(completed.Game.Words);
        experience = WithHintPenalty(experience, completed.HintsUsed, completed.Game.Words.Length);
        experience += SpeedBonus(completed);
        experience *= WeekendMultiplier(completed.CompletedAt);
        experience += StreakBonus(completed.StreakDays);
        experience += completed.IsFirstGameToday ? 20 : 0;
        experience = completed.IsPremium ? (int)(experience * 1.5) : experience;

        return Math.Min(experience, MaxExperience);
    }

    private static int WordExperience(string[] words) =>
        words.Sum(word => word.Length > 8 ? 15 : 10);

    // Using a hint for every word halves the experience, fewer hints cost 5 each
    private static int WithHintPenalty(int experience, int hintsUsed, int wordCount) => hintsUsed switch
    {
        0 => experience,
        _ when hintsUsed >= wordCount => experience / 2,
        _ => experience - hintsUsed * 5,
    };

    private static int SpeedBonus(CompletedGame completed) =>
        completed.HintsUsed == 0 && completed.CompletedAt - completed.Game.StartedAt < FastGame
            ? 5 * completed.Game.Words.Length
            : 0;

    private static int WeekendMultiplier(DateTimeOffset completedAt) =>
        completedAt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? 2 : 1;

    private static int StreakBonus(int streakDays) => streakDays switch
    {
        >= 7 => 50,
        >= 3 => 20,
        _ => 0,
    };
}
// #endregion after
