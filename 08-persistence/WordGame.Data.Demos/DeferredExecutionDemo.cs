namespace WordGame.Data.Demos;

public class DeferredExecutionDemo(DatabaseFixture database)
{
    // #region runs-when-enumerated
    [Fact]
    public void Query_RunsOnlyWhenEnumerated_AndEveryTime()
    {
        var log = new SqlLog();
        using var db = database.CreateContext(log);

        var query = db.Users.Where(u => u.Experience > 500); // Only a description of the query
        Assert.Equal(0, log.Count);

        var veterans = query.ToList();
        Assert.Equal(1, log.Count);

        var veteransAgain = query.ToList(); // Enumerating again runs the query again
        Assert.Equal(2, log.Count);
    }
    // #endregion runs-when-enumerated

    // #region where-runs
    [Fact]
    public void GamesOfOnePlayer_FilteredByDatabaseOrInMemory()
    {
        var inDatabase = Measure.Run(database, "IQueryable.Where",
            db => GamesOf(db.Games, userId: 1));

        var inMemory = Measure.Run(database, "IEnumerable.Where",
            db => GamesOfLoadingAll(db.Games, userId: 1));

        Report.Table(inDatabase, inMemory);

        Assert.Contains("WHERE", inDatabase.Sql.Single());
        Assert.DoesNotContain("WHERE", inMemory.Sql.Single()); // All 20 000 games are loaded to find 20
        Assert.True(inMemory.AllocatedBytes > 100 * inDatabase.AllocatedBytes);
    }

    private static List<Game> GamesOf(IQueryable<Game> games, int userId) =>
        games.Where(g => g.UserId == userId).ToList();

    // The only difference is the parameter type. Where on IEnumerable is LINQ to Objects:
    // a C# loop over every row, which have to be loaded from the database first.
    private static List<Game> GamesOfLoadingAll(IEnumerable<Game> games, int userId) =>
        games.Where(g => g.UserId == userId).ToList();
    // #endregion where-runs

    // #region untranslatable
    [Fact]
    public void CSharpMethod_CannotBeTranslatedToSql()
    {
        using var db = database.CreateContext();

        var error = Assert.Throws<InvalidOperationException>(() =>
            db.Users.Where(u => IsVeteran(u)).ToList());

        Report.WriteLine(error.Message);
    }

    // EF sees only a call to an unknown method, it cannot look inside it
    private static bool IsVeteran(User user) => user.Experience > 500;
    // #endregion untranslatable
}
