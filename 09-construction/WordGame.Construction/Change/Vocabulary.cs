using WordGame.Api;

namespace WordGame.Construction.Change;

// #region vocabulary
// Hides the decisions that are most likely to change: which language is learned,
// where the words come from and what counts as a correct answer.
// Adding a language or accepting small typos changes this class and nothing else.
public class Vocabulary(IReadOnlyDictionary<string, string> translations)
{
    public static Vocabulary EnglishToGerman { get; } = new(WordBank.EnglishToGerman);

    public IReadOnlyCollection<string> Words => [.. translations.Keys];

    public bool IsCorrect(string word, string answer) =>
        translations.TryGetValue(word, out var translation)
        && string.Equals(translation, answer.Trim(), StringComparison.OrdinalIgnoreCase);
}
// #endregion vocabulary

public static class Vocabularies
{
    // #region lithuanian
    public static Vocabulary EnglishToLithuanian { get; } = new(new Dictionary<string, string>
    {
        ["hello"] = "labas",
        ["world"] = "pasaulis",
        ["example"] = "pavyzdys",
        ["computer"] = "kompiuteris",
        ["programming"] = "programavimas",
        ["language"] = "kalba",
        ["game"] = "žaidimas",
    });
    // #endregion lithuanian
}
