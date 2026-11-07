using WordGame.Api;

namespace WordGame.Construction.Verification;

// #region before
// The same five words for every player on the same day, with a bonus for translating them.
// It works, but no test can check it: the words depend on today's date, and the bonus
// is written to a users.json file next to the running program.
public class DailyChallengeBefore
{
    public string[] TodaysWords()
    {
        var words = WordBank.EnglishToGerman.Keys.Order().ToArray();
        new Random(DateTime.Now.DayOfYear).Shuffle(words);
        return words[..5];
    }

    public bool Complete(string playerEmail, string[] answers)
    {
        var words = TodaysWords();
        var allCorrect = words.Zip(answers).All(pair =>
            string.Equals(WordBank.EnglishToGerman[pair.First], pair.Second.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!allCorrect)
        {
            return false;
        }

        new UserService(new JsonFileUserRepository("users.json")).AddExperience(playerEmail, 50);
        return true;
    }
}
// #endregion before
