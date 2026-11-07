namespace WordGame.Construction.Complexity;

// Two methods with the same cyclomatic complexity: one is read at a glance,
// the other has to be traced line by line.
public static class Readability
{
    // #region flat
    public static string LevelTitle(int level)
    {
        switch (level)
        {
            case < 5: return "Beginner";
            case < 10: return "Learner";
            case < 20: return "Speaker";
            default: return "Native";
        }
    }
    // #endregion flat

    // #region nested
    // Counts answers that match a translation, ignoring words without an answer
    public static int CountCorrect(string[] words, string[] answers, IReadOnlyDictionary<string, string> translations)
    {
        var correct = 0;
        for (var i = 0; i < words.Length; i++)
        {
            if (i < answers.Length)
            {
                if (translations.TryGetValue(words[i], out var translation))
                {
                    correct += string.Equals(translation, answers[i].Trim(), StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                }
            }
        }

        return correct;
    }
    // #endregion nested
}
