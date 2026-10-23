namespace WordGame.Data.Demos;

public class ConcurrencyDemo(DatabaseFixture database)
{
    // #region lost-update
    // The same player completes two games at the same time, and both requests award 30 XP
    // with read-modify-write code, like UserService.AddExperience in the testing lecture.
    [Fact]
    public void ReadModifyWrite_LosesAnUpdate()
    {
        var email = database.AddUser();
        using var firstRequest = database.CreateContext();
        using var secondRequest = database.CreateContext();

        var userInFirst = firstRequest.Users.Single(u => u.Email == email);   // Reads 0
        var userInSecond = secondRequest.Users.Single(u => u.Email == email); // Also reads 0

        userInFirst.Experience += 30;
        firstRequest.SaveChanges();  // Writes 30
        userInSecond.Experience += 30;
        secondRequest.SaveChanges(); // Writes 30 again

        Assert.Equal(30, database.ExperienceOf(email)); // Not 60: the first update was lost
    }
    // #endregion lost-update

    // #region atomic
    [Fact]
    public void AtomicUpdate_KeepsBothUpdates()
    {
        var email = database.AddUser();
        using var firstRequest = database.CreateContext();
        using var secondRequest = database.CreateContext();

        // UPDATE users SET experience = experience + 30: the database reads and writes in one step
        firstRequest.Users.Where(u => u.Email == email)
            .ExecuteUpdate(setters => setters.SetProperty(u => u.Experience, u => u.Experience + 30));
        secondRequest.Users.Where(u => u.Email == email)
            .ExecuteUpdate(setters => setters.SetProperty(u => u.Experience, u => u.Experience + 30));

        Assert.Equal(60, database.ExperienceOf(email));
    }
    // #endregion atomic

    // #region concurrency-token
    // Two admins fix the translation of the same word at the same time
    [Fact]
    public void ConcurrencyToken_DetectsConflictingEdit()
    {
        var wordId = database.AddWord();
        var log = new SqlLog();
        using var firstAdmin = database.CreateContext(log);
        using var secondAdmin = database.CreateContext();

        var wordForFirst = firstAdmin.Words.Single(w => w.Id == wordId);
        var wordForSecond = secondAdmin.Words.Single(w => w.Id == wordId);

        wordForFirst.German = "Hallo";
        firstAdmin.SaveChanges();

        // The UPDATE checks that xmin is still the value that was loaded. The first save changed it,
        // so no row is updated and EF throws instead of silently overwriting "Hallo".
        wordForSecond.German = "Servus";
        Assert.Throws<DbUpdateConcurrencyException>(() => secondAdmin.SaveChanges());
        Assert.Contains("WHERE id = @p1 AND xmin = @p2", log.Commands.Last());
    }
    // #endregion concurrency-token
}
