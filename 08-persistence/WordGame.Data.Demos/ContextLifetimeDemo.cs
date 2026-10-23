namespace WordGame.Data.Demos;

public class ContextLifetimeDemo(DatabaseFixture database)
{
    // #region stale
    [Fact]
    public void LongLivedContext_ReturnsStaleData()
    {
        var email = database.AddUser();
        var log = new SqlLog();
        using var longLived = database.CreateContext(log);

        var user = longLived.Users.Single(u => u.Email == email);

        using (var otherRequest = database.CreateContext())
        {
            otherRequest.Users.Single(u => u.Email == email).Experience = 100;
            otherRequest.SaveChanges();
        }

        var userAgain = longLived.Users.Single(u => u.Email == email);

        Assert.Equal(2, log.Count);              // The query did run and returned 100...
        Assert.Equal(0, userAgain.Experience);   // ...but the already tracked object was returned unchanged
        Assert.Equal(100, database.ExperienceOf(email));
    }
    // #endregion stale

    // #region shared
    // What happens when a context is registered as a singleton and two requests use it at once
    [Fact]
    public async Task SharedContext_CannotRunTwoQueriesAtOnce()
    {
        using var shared = database.CreateContext();
        var cancellation = TestContext.Current.CancellationToken;

        // Depending on timing either EF or the database driver notices the second query
        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            Task.WhenAll(shared.Users.ToListAsync(cancellation), shared.Games.ToListAsync(cancellation)));

        Report.WriteLine(error.Message);
    }
    // #endregion shared
}
