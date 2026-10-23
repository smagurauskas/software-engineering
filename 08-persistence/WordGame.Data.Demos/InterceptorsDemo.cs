using Microsoft.Extensions.Logging.Testing;

namespace WordGame.Data.Demos;

public class InterceptorsDemo(DatabaseFixture database)
{
    // #region audit
    [Fact]
    public void AuditInterceptor_SetsTimestamps()
    {
        var created = database.Time.GetUtcNow();
        var wordId = database.AddWord();
        database.Time.Advance(TimeSpan.FromDays(1));

        using (var db = database.CreateContext())
        {
            db.Words.Single(w => w.Id == wordId).German = "servus";
            db.SaveChanges(); // No code here sets UpdatedAt
        }

        using var check = database.CreateContext();
        var word = check.Words.Single(w => w.Id == wordId);
        Assert.Equal(created, word.CreatedAt);
        Assert.Equal(created.AddDays(1), word.UpdatedAt);
    }
    // #endregion audit

    // #region slow-query
    [Fact]
    public void SlowQueryInterceptor_LogsOnlySlowQueries_WithTheirTag()
    {
        var logger = new FakeLogger<SlowQueryInterceptor>();
        var slowQueries = new SlowQueryInterceptor(logger, threshold: TimeSpan.FromMilliseconds(100));
        using var db = database.CreateContext(interceptors: slowQueries);

        db.Users
            .TagWith("Leaderboard")
            .OrderByDescending(u => u.Experience)
            .Take(10)
            .ToList();

        db.Users
            .FromSql($"SELECT users.* FROM users, pg_sleep(0.2)") // Pretends to be a slow query
            .TagWith("Monthly report")
            .ToList();

        var logged = Assert.Single(logger.Collector.GetSnapshot());
        Assert.Contains("-- Monthly report", logged.Message);
        Report.WriteLine(logged.Message);
    }
    // #endregion slow-query
}
