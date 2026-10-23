namespace WordGame.Data.Demos;

public class SavingDemo(DatabaseFixture database)
{
    // #region batching
    [Fact]
    public void Adding100Users_SaveChangesPerUserOrOnce()
    {
        var perUser = Measure.Run(database, "SaveChanges per user", db =>
        {
            foreach (var user in NewUsers(100))
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
        }, rollback: true);

        var once = Measure.Run(database, "SaveChanges once", db =>
        {
            db.Users.AddRange(NewUsers(100));
            db.SaveChanges();
        }, rollback: true);

        Report.Table(perUser, once);

        // Every command is a round trip to the database. Here the database runs on the same machine;
        // in a data center every round trip takes about a millisecond, and across regions tens of them.
        Assert.Equal(100, perUser.Commands);
        Assert.Equal(1, once.Commands);
    }

    private static IEnumerable<User> NewUsers(int count) =>
        Enumerable.Range(0, count).Select(_ => new User { Email = $"{Guid.NewGuid()}@test.com" });
    // #endregion batching

    // #region bulk-update
    // A bonus of 10 XP for every player
    [Fact]
    public void BonusForEveryone_LoadAndSaveOrExecuteUpdate()
    {
        var loadAndSave = Measure.Run(database, "Load, change, SaveChanges", db =>
        {
            foreach (var user in db.Users.ToList())
            {
                user.Experience += 10;
            }
            db.SaveChanges();
        }, rollback: true);

        var executeUpdate = Measure.Run(database, "ExecuteUpdate", db =>
            db.Users.ExecuteUpdate(setters => setters.SetProperty(u => u.Experience, u => u.Experience + 10)),
            rollback: true);

        Report.Table(loadAndSave, executeUpdate);

        // One UPDATE statement, nothing is loaded. It bypasses the change tracker:
        // users already loaded into a context still have the old experience.
        Assert.Equal(1, executeUpdate.Commands);
        Assert.True(executeUpdate.AllocatedBytes * 10 < loadAndSave.AllocatedBytes);
    }
    // #endregion bulk-update
}
