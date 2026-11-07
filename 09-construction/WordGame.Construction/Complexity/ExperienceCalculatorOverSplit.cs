namespace WordGame.Construction.Complexity;

// #region over-split
// The same rules again, split until every method passes any complexity threshold.
// Each method is trivial, but to know what Calculate does all of them have to be
// read in order, keeping the running total in mind: the complexity was not removed,
// it was moved into the connections between methods.
public static class ExperienceCalculatorOverSplit
{
    public static int Calculate(CompletedGame completed) =>
        completed.Game.Words.Length == 0 ? 0 : Step1(completed, 0);

    private static int Step1(CompletedGame c, int xp) => Step2(c, xp + c.Game.Words.Sum(PerWord));

    private static int PerWord(string word) => word.Length > 8 ? 15 : 10;

    private static int Step2(CompletedGame c, int xp) => Step3(c, c.HintsUsed > 0 ? Hints(c, xp) : xp);

    private static int Hints(CompletedGame c, int xp) =>
        c.HintsUsed >= c.Game.Words.Length ? xp / 2 : xp - c.HintsUsed * 5;

    private static int Step3(CompletedGame c, int xp) => Step4(c, IsFast(c) ? xp + 5 * c.Game.Words.Length : xp);

    private static bool IsFast(CompletedGame c) =>
        c.HintsUsed == 0 && c.CompletedAt - c.Game.StartedAt < TimeSpan.FromMinutes(1);

    private static int Step4(CompletedGame c, int xp) => Step5(c, IsWeekend(c) ? xp * 2 : xp);

    private static bool IsWeekend(CompletedGame c) =>
        c.CompletedAt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static int Step5(CompletedGame c, int xp) => Step6(c, xp + Streak(c.StreakDays));

    private static int Streak(int days) => days >= 7 ? 50 : days >= 3 ? 20 : 0;

    private static int Step6(CompletedGame c, int xp) => Step7(c, c.IsFirstGameToday ? xp + 20 : xp);

    private static int Step7(CompletedGame c, int xp) => Math.Min(c.IsPremium ? (int)(xp * 1.5) : xp, 200);
}
// #endregion over-split
