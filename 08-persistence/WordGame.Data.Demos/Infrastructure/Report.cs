namespace WordGame.Data.Demos.Infrastructure;

// Writes to the test output, which `dotnet test` shows live (see xunit.runner.json)
public static class Report
{
    public static void WriteLine(string text) => TestContext.Current.TestOutputHelper?.WriteLine(text);

    public static void Table(params Measurement[] measurements)
    {
        var nameWidth = measurements.Max(m => m.Name.Length);
        WriteLine($"{"".PadRight(nameWidth)} | SQL commands |  Allocated |     Time");
        WriteLine($"{new string('-', nameWidth)}-|--------------|------------|---------");
        foreach (var m in measurements)
        {
            WriteLine($"{m.Name.PadRight(nameWidth)} | {m.Commands,12} | {m.AllocatedBytes / 1024.0,7:N0} KB | {m.Time.TotalMilliseconds,5:N1} ms");
        }

        foreach (var m in measurements)
        {
            WriteLine($"{Environment.NewLine}{m.Name}:{Environment.NewLine}{Shorten(m.Sql.FirstOrDefault() ?? "")}");
            if (m.Sql.Count > 1)
            {
                WriteLine($"... and {m.Sql.Count - 1} more commands");
            }
        }
    }

    // A batch of 100 INSERTs is one command, but there is no need to print all of it
    private static string Shorten(string sql, int maxLines = 12)
    {
        var lines = sql.Split('\n');
        return lines.Length <= maxLines
            ? sql
            : string.Join('\n', lines[..maxLines]) + $"\n... {lines.Length - maxLines} more lines";
    }
}
