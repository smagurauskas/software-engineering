using System.Diagnostics;

namespace WordGame.Data.Demos.Infrastructure;

public record Measurement(string Name, int Commands, long AllocatedBytes, TimeSpan Time, IReadOnlyList<string> Sql);

public static class Measure
{
    private const int Runs = 5;

    // Runs the code once to warm up (EF translates and caches the query on the first run),
    // then measures several runs, each with a new context, and reports the averages.
    // Allocations are counted on the current thread, so the measured code must be synchronous.
    // With `rollback` every run is in a transaction that is rolled back, so changes are not kept.
    public static Measurement Run(DatabaseFixture database, string name, Action<WordGameContext> action, bool rollback = false)
    {
        Execute(database, action, rollback, new SqlLog(print: false));

        var allocated = 0L;
        var time = TimeSpan.Zero;
        var log = new SqlLog(print: false);
        for (var run = 0; run < Runs; run++)
        {
            log = new SqlLog(print: false);
            var before = GC.GetAllocatedBytesForCurrentThread();
            var stopwatch = Stopwatch.StartNew();

            Execute(database, action, rollback, log);

            time += stopwatch.Elapsed;
            allocated += GC.GetAllocatedBytesForCurrentThread() - before;
        }

        return new Measurement(name, log.Count, allocated / Runs, time / Runs, log.Commands);
    }

    public static Measurement Run(DatabaseFixture database, string name, Func<WordGameContext, object?> query, bool rollback = false) =>
        Run(database, name, db => { query(db); }, rollback);

    private static void Execute(DatabaseFixture database, Action<WordGameContext> action, bool rollback, SqlLog log)
    {
        using var db = database.CreateContext(log);
        using var transaction = rollback ? db.Database.BeginTransaction() : null;

        action(db);
    }
}
