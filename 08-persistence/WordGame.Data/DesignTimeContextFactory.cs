using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WordGame.Data;

// Used only by the `dotnet ef` tool, which needs to create the context outside of an application.
// Adding a migration does not connect to the database; `dotnet ef database update` connects
// to the database started with `docker compose up -d` (compose.yaml).
public class DesignTimeContextFactory : IDesignTimeDbContextFactory<WordGameContext>
{
    public WordGameContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WordGameContext>();
        WordGameContext.Configure(
            options,
            "Host=localhost;Port=5432;Database=wordgame;Username=postgres;Password=devpassword",
            TimeProvider.System);

        return new WordGameContext(options.Options);
    }
}
