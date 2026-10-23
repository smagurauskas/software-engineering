namespace WordGame.Data.Demos;

public class SoftDeleteDemo(DatabaseFixture database)
{
    // #region remove
    [Fact]
    public void RemovedWord_IsHiddenButKept()
    {
        var (wordId, _) = AddWordUsedInGame();
        var log = new SqlLog();

        using (var db = database.CreateContext(log))
        {
            db.Words.Remove(db.Words.Single(w => w.Id == wordId));
            db.SaveChanges();
        }

        // Remove was turned into an UPDATE by SoftDeleteInterceptor
        Assert.StartsWith("UPDATE words SET deleted_at = @p0", log.Commands.Last());

        using var check = database.CreateContext();
        Assert.False(check.Words.Any(w => w.Id == wordId)); // The query filter hides it...
        Assert.True(check.Words.IgnoreQueryFilters().Any(w => w.Id == wordId)); // ...but the row is there
    }
    // #endregion remove

    // #region include
    [Fact]
    public void SoftDeletedWord_IsHiddenFromOldGamesToo()
    {
        var (wordId, gameId) = AddWordUsedInGame();
        DeleteWord(wordId);

        using var db = database.CreateContext();

        // The query filter applies to included entities as well
        var game = db.Games.Include(g => g.Words).Single(g => g.Id == gameId);
        Assert.Empty(game.Words);

        // A game history screen has to ask for deleted words explicitly
        var history = db.Games.IgnoreQueryFilters().Include(g => g.Words).Single(g => g.Id == gameId);
        Assert.Single(history.Words);
    }
    // #endregion include

    // #region execute-delete
    [Fact]
    public void ExecuteDelete_BypassesInterceptorsAndCascades()
    {
        var (wordId, gameId) = AddWordUsedInGame();
        using var db = database.CreateContext();

        // A single DELETE statement: no entities are loaded, so SaveChanges interceptors never run
        db.Words.Where(w => w.Id == wordId).ExecuteDelete();

        Assert.False(db.Words.IgnoreQueryFilters().Any(w => w.Id == wordId)); // Really deleted
        // The foreign key of game_word has ON DELETE CASCADE: the old game lost its word as well
        var game = db.Games.IgnoreQueryFilters().Include(g => g.Words).Single(g => g.Id == gameId);
        Assert.Empty(game.Words);
    }
    // #endregion execute-delete

    private (int WordId, int GameId) AddWordUsedInGame()
    {
        var wordId = database.AddWord();
        var email = database.AddUser();
        using var db = database.CreateContext();
        var game = new Game
        {
            User = db.Users.Single(u => u.Email == email),
            StartedAt = database.Time.GetUtcNow(),
            Words = [db.Words.Single(w => w.Id == wordId)],
        };
        db.Games.Add(game);
        db.SaveChanges();

        return (wordId, game.Id);
    }

    private void DeleteWord(int wordId)
    {
        using var db = database.CreateContext();
        db.Words.Remove(db.Words.Single(w => w.Id == wordId));
        db.SaveChanges();
    }
}
