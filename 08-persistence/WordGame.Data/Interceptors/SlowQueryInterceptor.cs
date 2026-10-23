using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace WordGame.Data.Interceptors;

// #region slow-query
// Runs after every query and logs the ones that took longer than the threshold.
// Queries marked with TagWith("...") carry the tag as an SQL comment, so the log shows where they came from.
public class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger, TimeSpan threshold) : DbCommandInterceptor
{
    public override DbDataReader ReaderExecuted(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        LogIfSlow(command, eventData.Duration);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration);
        return ValueTask.FromResult(result);
    }

    private void LogIfSlow(DbCommand command, TimeSpan duration)
    {
        if (duration > threshold)
        {
            logger.LogWarning("Slow query ({Milliseconds} ms):\n{Sql}", (int)duration.TotalMilliseconds, command.CommandText);
        }
    }
}
// #endregion slow-query
