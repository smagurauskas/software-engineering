using WordGame.Api;
using WordGame.Construction.Change;

namespace WordGame.Construction.Verification;

// #region after
// Everything the result depends on comes in through the constructor or the parameters,
// so a test controls all of it. The day is a parameter: a player who opened the
// challenge before midnight still gets the words they were shown.
public class DailyChallenge(Vocabulary vocabulary, UserService users, TimeProvider time)
{
    public const int WordCount = 5;
    public const int Bonus = 50;

    public DateOnly Today => DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);

    public string[] WordsFor(DateOnly day)
    {
        var words = vocabulary.Words.Order().ToArray();
        new Random(day.DayNumber).Shuffle(words);
        return words[..WordCount];
    }

    public bool Complete(string playerEmail, DateOnly day, string[] answers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerEmail);
        ArgumentNullException.ThrowIfNull(answers);

        var words = WordsFor(day);
        var allCorrect = answers.Length == words.Length
            && words.Zip(answers).All(pair => vocabulary.IsCorrect(pair.First, pair.Second));
        if (allCorrect)
        {
            users.AddExperience(playerEmail, Bonus);
        }

        return allCorrect;
    }
}
// #endregion after
