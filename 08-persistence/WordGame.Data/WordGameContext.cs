using Microsoft.EntityFrameworkCore;
using WordGame.Data.Interceptors;

namespace WordGame.Data;

// #region context
public class WordGameContext(DbContextOptions<WordGameContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Word> Words => Set<Word>();

    // Only what conventions cannot guess has to be configured
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>().Property(u => u.Email).HasMaxLength(254);
        model.Entity<User>().HasIndex(u => u.Email).IsUnique();

        model.Entity<Game>().HasIndex(g => g.StartedAt);
        model.Entity<Game>().HasMany(g => g.Words).WithMany();

        model.Entity<Word>().Property(w => w.English).HasMaxLength(100);
        model.Entity<Word>().Property(w => w.German).HasMaxLength(100);
        // Added to every query of words, so soft-deleted words are never returned by accident
        model.Entity<Word>().HasQueryFilter(w => w.DeletedAt == null);
    }
    // #endregion context

    // #region configure
    // The same configuration is used by the application, the tests and the `dotnet ef` tool
    public static void Configure(DbContextOptionsBuilder options, string connectionString, TimeProvider time) =>
        options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            // In this order: a removed word first becomes a modified one, then gets its UpdatedAt
            .AddInterceptors(new SoftDeleteInterceptor(time), new AuditInterceptor(time))
            // Runs after migrations are applied (by Migrate or `dotnet ef database update`)
            .UseSeeding((context, _) =>
            {
                if (!context.Set<Word>().IgnoreQueryFilters().Any())
                {
                    context.Set<Word>().AddRange(StarterWords());
                    context.SaveChanges();
                }
            })
            .UseAsyncSeeding(async (context, _, cancellation) =>
            {
                if (!await context.Set<Word>().IgnoreQueryFilters().AnyAsync(cancellation))
                {
                    context.Set<Word>().AddRange(StarterWords());
                    await context.SaveChangesAsync(cancellation);
                }
            });

    private static IEnumerable<Word> StarterWords() =>
        new Dictionary<string, string>
        {
            ["hello"] = "hallo",
            ["world"] = "welt",
            ["example"] = "beispiel",
            ["computer"] = "computer",
            ["programming"] = "programmierung",
            ["language"] = "sprache",
            ["game"] = "spiel",
        }.Select(pair => new Word { English = pair.Key, German = pair.Value });
    // #endregion configure
}
