using WordGame.Api;

namespace WordGame.Testing;

// Test double for the random word picker: always picks the given words,
// so a test knows which translations are correct.
public class FixedWordPicker(params string[] words) : IWordPicker
{
    public string[] Pick(int count) => words[..count];
}
