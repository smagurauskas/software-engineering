namespace WordGame.Construction.Complexity;

// #region before
// The experience rules after a year of feature requests. Every rule was added
// to the method that already existed, one `if` at a time: each change was small
// and reasonable on its own.
public static class ExperienceCalculatorBefore
{
    public static int Calculate(CompletedGame completed)
    {
        var experience = 0;
        if (completed.Game.Words.Length > 0)
        {
            foreach (var word in completed.Game.Words)
            {
                if (word.Length > 8)
                {
                    experience += 15;
                }
                else
                {
                    experience += 10;
                }
            }

            if (completed.HintsUsed > 0)
            {
                if (completed.HintsUsed >= completed.Game.Words.Length)
                {
                    experience = experience / 2;
                }
                else
                {
                    experience -= completed.HintsUsed * 5;
                }
            }

            var duration = completed.CompletedAt - completed.Game.StartedAt;
            if (duration < TimeSpan.FromMinutes(1) && completed.HintsUsed == 0)
            {
                experience += 5 * completed.Game.Words.Length;
            }

            var day = completed.CompletedAt.DayOfWeek;
            if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
            {
                experience *= 2;
            }

            if (completed.StreakDays > 0)
            {
                if (completed.StreakDays >= 7)
                {
                    experience += 50;
                }
                else if (completed.StreakDays >= 3)
                {
                    experience += 20;
                }
            }

            if (completed.IsFirstGameToday)
            {
                experience += 20;
            }

            if (completed.IsPremium)
            {
                experience = (int)(experience * 1.5);
            }

            if (experience > 200)
            {
                experience = 200;
            }
        }

        return experience;
    }
}
// #endregion before
