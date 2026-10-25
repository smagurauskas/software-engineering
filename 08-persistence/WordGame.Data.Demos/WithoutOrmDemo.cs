using System.Text;
using Npgsql;

namespace WordGame.Data.Demos;

// The same operations written with plain ADO.NET (the Npgsql driver, which EF itself uses underneath)
// and with EF. Every test checks that both versions give the same result.
public class WithoutOrmDemo(DatabaseFixture database)
{
    // #region read-ado
    private User? FindUserAdo(string email)
    {
        using var connection = new NpgsqlConnection(database.ConnectionString);
        connection.Open();

        using var command = new NpgsqlCommand("SELECT id, email, experience FROM users WHERE email = @email", connection);
        command.Parameters.AddWithValue("email", email);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        // Every column is read and converted by hand, for every query that returns users
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Email = reader.GetString(reader.GetOrdinal("email")),
            Experience = reader.GetInt32(reader.GetOrdinal("experience")),
        };
    }
    // #endregion read-ado

    // #region read-ef
    private User? FindUserEf(string email)
    {
        using var db = database.CreateContext();
        return db.Users.SingleOrDefault(u => u.Email == email);
    }
    // #endregion read-ef

    [Fact]
    public void FindUser_SameResult()
    {
        var ado = FindUserAdo("player1@test.com")!;
        var ef = FindUserEf("player1@test.com")!;

        Assert.Equal((ef.Id, ef.Email, ef.Experience), (ado.Id, ado.Email, ado.Experience));
    }

    // #region typo
    [Fact]
    public void TypoInSql_CompilesButFailsWhenRun()
    {
        using var connection = new NpgsqlConnection(database.ConnectionString);
        connection.Open();
        using var command = new NpgsqlCommand("SELECT id, email, experiance FROM users", connection);

        // The SQL is just a string: the compiler cannot check it, so the mistake is found only when
        // this line runs - hopefully in a test, not in production. With EF `u.Experiance` does not
        // compile, and renaming the property in the IDE updates every query that uses it.
        var error = Assert.Throws<PostgresException>(() => command.ExecuteReader());
        Report.WriteLine(error.MessageText);
    }
    // #endregion typo

    // #region graph-ado
    private User LoadUserWithGamesAdo(string email)
    {
        using var connection = new NpgsqlConnection(database.ConnectionString);
        connection.Open();

        using var command = new NpgsqlCommand(
            """
            SELECT u.id, u.email, u.experience, g.id, g.started_at, g.experience
            FROM users u
            LEFT JOIN games g ON g.user_id = u.id
            WHERE u.email = @email
            """,
            connection);
        command.Parameters.AddWithValue("email", email);

        // The result is flat: one row per game, with the user's columns repeated on every row
        using var reader = command.ExecuteReader();
        User? user = null;
        while (reader.Read())
        {
            user ??= new User { Id = reader.GetInt32(0), Email = reader.GetString(1), Experience = reader.GetInt32(2) };

            // A user without games still gets one row, with NULLs in the game columns
            if (!reader.IsDBNull(3))
            {
                user.Games.Add(new Game
                {
                    Id = reader.GetInt32(3),
                    StartedAt = reader.GetFieldValue<DateTimeOffset>(4),
                    Experience = reader.GetInt32(5),
                    UserId = user.Id,
                    User = user,
                });
            }
        }

        return user ?? throw new InvalidOperationException($"No user {email}");
    }
    // #endregion graph-ado

    // #region graph-ef
    private User LoadUserWithGamesEf(string email)
    {
        using var db = database.CreateContext();
        return db.Users.Include(u => u.Games).Single(u => u.Email == email);
    }
    // #endregion graph-ef

    [Fact]
    public void LoadUserWithGames_SameResult()
    {
        var ado = LoadUserWithGamesAdo("player1@test.com");
        var ef = LoadUserWithGamesEf("player1@test.com");

        Assert.Equal(ef.Games.Select(g => g.Id).Order(), ado.Games.Select(g => g.Id).Order());
    }

    // #region save-ado
    // A new player with their first game: the game needs the id the database generates for the user,
    // and both INSERTs have to succeed or fail together
    private void AddPlayerWithGameAdo(string email, int experience)
    {
        using var connection = new NpgsqlConnection(database.ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using var insertUser = new NpgsqlCommand(
            "INSERT INTO users (email, experience) VALUES (@email, @experience) RETURNING id", connection, transaction);
        insertUser.Parameters.AddWithValue("email", email);
        insertUser.Parameters.AddWithValue("experience", experience);
        var userId = (int)insertUser.ExecuteScalar()!;

        using var insertGame = new NpgsqlCommand(
            "INSERT INTO games (user_id, started_at, experience) VALUES (@userId, @startedAt, @experience)",
            connection,
            transaction);
        insertGame.Parameters.AddWithValue("userId", userId);
        insertGame.Parameters.AddWithValue("startedAt", DateTimeOffset.UtcNow);
        insertGame.Parameters.AddWithValue("experience", experience);
        insertGame.ExecuteNonQuery();

        transaction.Commit();
    }
    // #endregion save-ado

    // #region save-ef
    private void AddPlayerWithGameEf(string email, int experience)
    {
        using var db = database.CreateContext();
        db.Users.Add(new User
        {
            Email = email,
            Experience = experience,
            Games = [new Game { StartedAt = DateTimeOffset.UtcNow, Experience = experience }],
        });

        // Both INSERTs in one transaction, and the user's new id is copied into the game
        db.SaveChanges();
    }
    // #endregion save-ef

    [Fact]
    public void AddPlayerWithGame_SameResult()
    {
        var adoEmail = $"{Guid.NewGuid()}@test.com";
        var efEmail = $"{Guid.NewGuid()}@test.com";

        AddPlayerWithGameAdo(adoEmail, 30);
        AddPlayerWithGameEf(efEmail, 30);

        using var db = database.CreateContext();
        Assert.Single(db.Users.Include(u => u.Games).Single(u => u.Email == adoEmail).Games);
        Assert.Single(db.Users.Include(u => u.Games).Single(u => u.Email == efEmail).Games);
    }

    // #region search
    // A player search screen where every filter is optional
    public record PlayerSearch(string? EmailContains, int? MinExperience, bool BestFirst);
    // #endregion search

    // #region search-ado
    private List<string> SearchAdo(PlayerSearch search)
    {
        using var connection = new NpgsqlConnection(database.ConnectionString);
        connection.Open();
        using var command = new NpgsqlCommand { Connection = connection };

        // The SQL is assembled piece by piece, and every piece needs its parameter added separately
        var sql = new StringBuilder("SELECT email FROM users WHERE 1 = 1");
        if (search.EmailContains is not null)
        {
            sql.Append(" AND email LIKE @email");
            command.Parameters.AddWithValue("email", $"%{search.EmailContains}%");
        }
        if (search.MinExperience is not null)
        {
            sql.Append(" AND experience >= @minExperience");
            command.Parameters.AddWithValue("minExperience", search.MinExperience.Value);
        }
        sql.Append(search.BestFirst ? " ORDER BY experience DESC, id" : " ORDER BY id");
        sql.Append(" LIMIT 20");
        command.CommandText = sql.ToString();

        using var reader = command.ExecuteReader();
        var emails = new List<string>();
        while (reader.Read())
        {
            emails.Add(reader.GetString(0));
        }
        return emails;
    }
    // #endregion search-ado

    // #region search-ef
    private List<string> SearchEf(PlayerSearch search)
    {
        using var db = database.CreateContext();

        // Queries are composed like any other C# objects: nothing runs until ToList
        var query = db.Users.AsQueryable();
        if (search.EmailContains is not null)
        {
            query = query.Where(u => u.Email.Contains(search.EmailContains));
        }
        if (search.MinExperience is not null)
        {
            query = query.Where(u => u.Experience >= search.MinExperience);
        }
        query = search.BestFirst
            ? query.OrderByDescending(u => u.Experience).ThenBy(u => u.Id)
            : query.OrderBy(u => u.Id);

        return query.Select(u => u.Email).Take(20).ToList();
    }
    // #endregion search-ef

    [Fact]
    public void Search_SameResult()
    {
        var search = new PlayerSearch(EmailContains: "player1", MinExperience: 400, BestFirst: true);

        Assert.Equal(SearchEf(search), SearchAdo(search));
    }

    // #region wildcard
    [Fact]
    public void SearchForUnderscore_HandWrittenLikeMatchesEverything()
    {
        // No email contains "_", but in LIKE an underscore means "any one character"
        var search = new PlayerSearch(EmailContains: "_", MinExperience: null, BestFirst: false);

        Assert.Equal(20, SearchAdo(search).Count); // Every user matches - a bug that is easy to miss
        Assert.Empty(SearchEf(search));            // EF escapes the special characters of LIKE
    }
    // #endregion wildcard
}
