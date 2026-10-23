using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WordGame.Data.Demos.Infrastructure;

// #region sql-log
// Collects every SQL command a context sends to the database, so the demos can count them,
// and writes it to the test output (with parameter values), so the SQL can be read.
public class SqlLog(bool print = true)
{
    private readonly List<string> _commands = [];

    public IReadOnlyList<string> Commands => _commands;

    public int Count => _commands.Count;

    public void AttachTo(DbContextOptionsBuilder options) =>
        options
            .EnableSensitiveDataLogging() // Shows parameter values in the log. Never in production: they can be passwords.
            .LogTo(
                (eventId, _) => eventId == RelationalEventId.CommandExecuted,
                eventData =>
                {
                    _commands.Add(((CommandExecutedEventData)eventData).Command.CommandText);
                    if (print)
                    {
                        Report.WriteLine(eventData.ToString() + Environment.NewLine);
                    }
                });
}
// #endregion sql-log
