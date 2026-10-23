namespace WordGame.Data.Demos;

public class ChangeTrackingDemo(DatabaseFixture database)
{
    // #region update
    [Fact]
    public void ChangedProperty_IsSavedWithoutCallingUpdate()
    {
        var email = database.AddUser();
        var log = new SqlLog();
        using var db = database.CreateContext(log);

        var user = db.Users.Single(u => u.Email == email);
        user.Experience += 30; // The context remembers the loaded values and notices the change
        db.SaveChanges();

        // Only the changed column is updated
        Assert.Contains("UPDATE users SET experience = @p0", log.Commands.Last());
    }
    // #endregion update

    // #region identity
    [Fact]
    public void TrackedQueries_ReturnTheSameInstance()
    {
        using var db = database.CreateContext();

        var first = db.Users.Single(u => u.Email == "player1@test.com");
        var second = db.Users.Single(u => u.Email == "player1@test.com");
        Assert.Same(first, second); // One row is one object in a context

        var notTracked = db.Users.AsNoTracking().Single(u => u.Email == "player1@test.com");
        Assert.NotSame(first, notTracked);
    }
    // #endregion identity

    // #region tracked-cost
    [Fact]
    public void LoadingAllGames_TrackedAndNotTracked()
    {
        var tracked = Measure.Run(database, "Tracked", db => db.Games.ToList());
        var notTracked = Measure.Run(database, "AsNoTracking", db => db.Games.AsNoTracking().ToList());

        Report.Table(tracked, notTracked);

        // The SQL is the same, the difference is what EF does with every loaded row
        Assert.Equal(tracked.Sql.Single(), notTracked.Sql.Single());
        Assert.True(notTracked.AllocatedBytes < tracked.AllocatedBytes);
    }

    [Fact]
    public void Tracking_KeepsEveryLoadedEntityInTheContext()
    {
        using var db = database.CreateContext();

        var games = db.Games.ToList();
        Assert.Equal(games.Count, db.ChangeTracker.Entries().Count()); // A copy of every value is kept too

        db.ChangeTracker.Clear();
        db.Games.AsNoTracking().ToList();
        Assert.Empty(db.ChangeTracker.Entries());
    }
    // #endregion tracked-cost

    // #region not-tracked-not-saved
    [Fact]
    public void NotTrackedChanges_AreNotSaved()
    {
        var email = database.AddUser();
        using var db = database.CreateContext();

        var user = db.Users.AsNoTracking().Single(u => u.Email == email);
        user.Experience += 30;
        var savedRows = db.SaveChanges();

        Assert.Equal(0, savedRows); // The context does not know about the user, nothing is saved
        Assert.Equal(0, database.ExperienceOf(email));
    }
    // #endregion not-tracked-not-saved
}
