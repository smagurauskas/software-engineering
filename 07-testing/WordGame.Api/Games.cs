using System.Collections.Concurrent;

namespace WordGame.Api;

public record Game(string PlayerEmail, string[] Words, DateTimeOffset StartedAt);

public interface IGameStore
{
    // Saving a game replaces the player's previous game
    void Save(Game game);
    Game? Find(string playerEmail);
    void Remove(string playerEmail);
}

// An instance field rather than a static one: every test that creates its own store
// (or its own application) starts from a clean state.
public class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<string, Game> _games = new();

    public void Save(Game game) => _games[game.PlayerEmail] = game;

    public Game? Find(string playerEmail) => _games.GetValueOrDefault(playerEmail);

    public void Remove(string playerEmail) => _games.TryRemove(playerEmail, out _);
}

public class GameService(IWordPicker wordPicker, IGameStore games, UserService users, TimeProvider time)
{
    public const int WordsPerGame = 3;
    public const int ExperiencePerWord = 10;
    public static readonly TimeSpan TimeLimit = TimeSpan.FromMinutes(5);

    public StartGameResponse StartGame(string playerEmail)
    {
        var words = wordPicker.Pick(WordsPerGame);
        games.Save(new Game(playerEmail, words, time.GetUtcNow()));

        return new StartGameResponse(words);
    }

    // #region complete-game
    public CompleteGameResponse CompleteGame(string playerEmail, string[] translations)
    {
        var game = games.Find(playerEmail);
        if (game is null || IsExpired(game) || !AreCorrect(game.Words, translations))
        {
            return new CompleteGameResponse(false, 0);
        }

        // The game is removed, so submitting the same answers again awards nothing
        games.Remove(playerEmail);

        var experience = CalculateExperience(game);
        users.AddExperience(playerEmail, experience);

        return new CompleteGameResponse(true, experience);
    }
    // #endregion complete-game

    private bool IsExpired(Game game) => time.GetUtcNow() - game.StartedAt > TimeLimit;

    private static bool AreCorrect(string[] words, string[] translations) =>
        words.Length == translations.Length
        && words.Zip(translations).All(pair =>
            string.Equals(WordBank.EnglishToGerman[pair.First], pair.Second.Trim(), StringComparison.OrdinalIgnoreCase));

    // #region experience
    // Games completed on a weekend award double experience
    private int CalculateExperience(Game game)
    {
        var experience = game.Words.Length * ExperiencePerWord;
        var day = time.GetUtcNow().DayOfWeek;

        return day is DayOfWeek.Saturday or DayOfWeek.Sunday ? experience * 2 : experience;
    }
    // #endregion experience
}
