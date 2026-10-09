namespace WordGame.Api;

public static class WordBank
{
    public static readonly IReadOnlyDictionary<string, string> EnglishToGerman = new Dictionary<string, string>
    {
        ["hello"] = "hallo",
        ["world"] = "welt",
        ["example"] = "beispiel",
        ["computer"] = "computer",
        ["programming"] = "programmierung",
        ["language"] = "sprache",
        ["game"] = "spiel",
    };
}

// Picking words is behind an interface, because randomness cannot be controlled by tests:
// a test has to know which words were picked to know the correct translations.
public interface IWordPicker
{
    string[] Pick(int count);
}

public class RandomWordPicker : IWordPicker
{
    public string[] Pick(int count)
    {
        var words = WordBank.EnglishToGerman.Keys.ToArray();
        Random.Shared.Shuffle(words);
        return words[..count];
    }
}
