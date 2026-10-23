using System.Diagnostics;

namespace WordGame.Data.Demos;

public class CancellationDemo(DatabaseFixture database)
{
    // #region cancel
    // In an endpoint the token comes from the request: ASP.NET cancels it when the client disconnects.
    // app.MapGet("/api/report", async (WordGameContext db, CancellationToken cancellation) => ...)
    [Fact]
    public async Task CancelledToken_StopsTheQueryInTheDatabase()
    {
        using var db = database.CreateContext();
        using var closedBrowserTab = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var stopwatch = Stopwatch.StartNew();

        // A report that would take 10 seconds
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            db.Database.ExecuteSqlAsync($"SELECT pg_sleep(10)", closedBrowserTab.Token));

        // Npgsql asks PostgreSQL to cancel the query, so the database stops working on it too
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2));
    }
    // #endregion cancel
}
