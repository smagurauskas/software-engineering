namespace WordGame.Data.Demos;

public class TransactionDemo(DatabaseFixture database)
{
    // #region save-changes
    [Fact]
    public void SaveChanges_SavesAllOrNothing()
    {
        var newEmail = $"{Guid.NewGuid()}@test.com";
        using (var db = database.CreateContext())
        {
            db.Users.Add(new User { Email = newEmail });
            db.Users.Add(new User { Email = "player1@test.com" }); // Already exists: violates the unique index

            Assert.Throws<DbUpdateException>(() => db.SaveChanges());
        }

        using var check = database.CreateContext();
        Assert.False(check.Users.Any(u => u.Email == newEmail)); // The valid user was not saved either
    }
    // #endregion save-changes

    // #region explicit
    // Saving a game and adding its experience are two commands. Without a transaction a failure
    // between them would leave a saved game whose experience was never added.
    private void CompleteGame(string email, int experience, bool failBeforeCommit)
    {
        using var db = database.CreateContext();
        using var transaction = db.Database.BeginTransaction();

        var user = db.Users.Single(u => u.Email == email);
        db.Games.Add(new Game { User = user, StartedAt = DateTimeOffset.UtcNow, Experience = experience });
        db.SaveChanges();

        db.Users.Where(u => u.Id == user.Id)
            .ExecuteUpdate(setters => setters.SetProperty(u => u.Experience, u => u.Experience + experience));

        if (failBeforeCommit)
        {
            throw new TimeoutException("Simulated failure");
        }

        transaction.Commit(); // Without Commit, disposing the transaction rolls everything back
    }

    [Fact]
    public void FailureBeforeCommit_RollsBackBothCommands()
    {
        var email = database.AddUser();

        Assert.Throws<TimeoutException>(() => CompleteGame(email, 30, failBeforeCommit: true));

        using var check = database.CreateContext();
        var user = check.Users.Include(u => u.Games).Single(u => u.Email == email);
        Assert.Empty(user.Games);
        Assert.Equal(0, user.Experience);
    }
    // #endregion explicit

    [Fact]
    public void Commit_SavesBothCommands()
    {
        var email = database.AddUser();

        CompleteGame(email, 30, failBeforeCommit: false);

        using var check = database.CreateContext();
        var user = check.Users.Include(u => u.Games).Single(u => u.Email == email);
        Assert.Single(user.Games);
        Assert.Equal(30, user.Experience);
    }
}
