using System.Data;
using System.Text.RegularExpressions;

namespace WordGame.Data.Demos.Infrastructure;

// #region explain
// Asks PostgreSQL how it runs the SQL generated for a LINQ query. EXPLAIN ANALYZE really runs it
// and reports what it did: which index or table scan it used and how many rows each step read.
public static partial class QueryPlan
{
    public static string Explain(IQueryable query)
    {
        // The command EF would send, with its parameters
        using var command = query.CreateDbCommand();
        command.CommandText = "EXPLAIN (ANALYZE, COSTS OFF) " + command.CommandText;
        if (command.Connection!.State != ConnectionState.Open)
        {
            command.Connection.Open();
        }

        using var reader = command.ExecuteReader();
        var lines = new List<string>();
        while (reader.Read())
        {
            lines.Add(reader.GetString(0));
        }

        var plan = string.Join(Environment.NewLine, lines);
        Report.WriteLine(plan + Environment.NewLine);
        return plan;
    }
    // #endregion explain

    // The most rows any step of the plan read
    public static int RowsRead(string plan) =>
        RowsPattern().Matches(plan).Max(match => int.Parse(match.Groups[1].Value));

    [GeneratedRegex(@"rows=(\d+)")]
    private static partial Regex RowsPattern();
}
