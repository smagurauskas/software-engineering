namespace WordGame.Data.Demos;

public class IndexesDemo(DatabaseFixture database)
{
    // #region foreign-key
    [Fact]
    public void GamesOfOnePlayer_UseTheForeignKeyIndex()
    {
        using var db = database.CreateContext();

        var plan = QueryPlan.Explain(db.Games.Where(g => g.UserId == 1));

        // EF creates an index for every foreign key by convention (see the InitialCreate migration)
        Assert.Contains("ix_games_user_id", plan);
    }
    // #endregion foreign-key

    // #region with-and-without
    [Fact]
    public void GamesOfOneDay_WithAndWithoutIndex()
    {
        using var db = database.CreateContext();
        using var transaction = db.Database.BeginTransaction();

        var day = new DateTimeOffset(2026, 9, 15, 0, 0, 0, TimeSpan.Zero);
        var gamesOfDay = db.Games.Where(g => g.StartedAt >= day && g.StartedAt < day.AddDays(1));

        var withIndex = QueryPlan.Explain(gamesOfDay);

        // Schema changes are transactional in PostgreSQL: the index is back after the rollback
        db.Database.ExecuteSql($"DROP INDEX ix_games_started_at");
        var withoutIndex = QueryPlan.Explain(gamesOfDay);

        Assert.Contains("ix_games_started_at", withIndex);
        Assert.Contains("Seq Scan on games", withoutIndex); // Every row of the table is read and checked
    }
    // #endregion with-and-without

    // #region function
    [Fact]
    public void FunctionOnColumn_CannotUseTheIndex()
    {
        using var db = database.CreateContext();
        var email = "player1@test.com";

        var exact = QueryPlan.Explain(db.Users.Where(u => u.Email == email));
        var lowered = QueryPlan.Explain(db.Users.Where(u => u.Email.ToLower() == email));

        // The index contains emails, not lower(email), so it cannot be used for the second query.
        // Store emails in lower case when saving them instead.
        Assert.Contains("ix_users_email", exact);
        Assert.Contains("Seq Scan on users", lowered);
    }
    // #endregion function
}
