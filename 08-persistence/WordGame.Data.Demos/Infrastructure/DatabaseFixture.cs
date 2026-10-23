using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(WordGame.Data.Demos.Infrastructure.DatabaseFixture))]

namespace WordGame.Data.Demos.Infrastructure;

// #region fixture
// One PostgreSQL container for the whole test run: started in Docker before the first test,
// migrated with the project's migrations, filled with data and removed after the last test.
public class DatabaseFixture : IAsyncLifetime
{
    public const int UserCount = 1_000;
    public const int GamesPerUser = 20;
    public const int WordsPerGame = 3;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

    // The clock used by the interceptors, so tests can check and move the time
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 10, 5, 12, 0, 0, TimeSpan.Zero));

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        using var db = CreateContext();
        db.Database.Migrate();
        Seed(db);

        // Updates the statistics the query planner uses to choose between indexes and table scans.
        // PostgreSQL does it on its own after enough changes, but not right after a bulk insert.
        db.Database.ExecuteSql($"ANALYZE");
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();

    // A new context for every unit of work, the same way a web application creates one per request.
    // Pass a SqlLog to see and count the SQL commands the context sends.
    public WordGameContext CreateContext(SqlLog? log = null, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<WordGameContext>();
        WordGameContext.Configure(options, _container.GetConnectionString(), Time);
        log?.AttachTo(options);
        options.AddInterceptors(interceptors);

        return new WordGameContext(options.Options);
    }
    // #endregion fixture

    // Demos that change data work with their own user or word, so the seeded data stays the same

    public string AddUser()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        using var db = CreateContext();
        db.Users.Add(new User { Email = email });
        db.SaveChanges();

        return email;
    }

    public int AddWord()
    {
        using var db = CreateContext();
        var word = new Word { English = "hello", German = "hallo" };
        db.Words.Add(word);
        db.SaveChanges();

        return word.Id;
    }

    public int ExperienceOf(string email)
    {
        using var db = CreateContext();
        return db.Users.Single(u => u.Email == email).Experience;
    }

    // Players player1@test.com ... player1000@test.com, each with 20 games of 3 words
    private static void Seed(WordGameContext db)
    {
        var random = new Random(42);
        var words = Enumerable.Range(1, 100)
            .Select(i => new Word { English = $"word-{i}", German = $"wort-{i}" })
            .ToArray();
        var firstGame = new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);

        for (var i = 1; i <= UserCount; i++)
        {
            var user = new User { Email = $"player{i}@test.com" };
            for (var g = 0; g < GamesPerUser; g++)
            {
                user.Games.Add(new Game
                {
                    StartedAt = firstGame.AddHours(i + g * 24),
                    Experience = random.Next(1, WordsPerGame + 1) * 10,
                    Words = [.. random.GetItems(words, 10).Distinct().Take(WordsPerGame)],
                });
            }

            user.Experience = user.Games.Sum(g => g.Experience);
            db.Users.Add(user);
        }

        db.SaveChanges();
    }
}
