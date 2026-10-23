namespace WordGame.Data.Demos;

public class MigrationsDemo(DatabaseFixture database)
{
    // #region applied
    [Fact]
    public void AllMigrations_AreApplied()
    {
        using var db = database.CreateContext();

        // EF records applied migrations in the __EFMigrationsHistory table
        var applied = db.Database.GetAppliedMigrations().ToList();
        applied.ForEach(Report.WriteLine);

        Assert.Equal(db.Database.GetMigrations(), applied);
    }
    // #endregion applied

    // #region pending
    // Fails when an entity was changed but `dotnet ef migrations add` was not run,
    // so a forgotten migration is caught by CI instead of in production.
    [Fact]
    public void Model_HasNoChangesWithoutAMigration()
    {
        using var db = database.CreateContext();

        Assert.False(db.Database.HasPendingModelChanges());
    }
    // #endregion pending
}
