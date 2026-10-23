namespace WordGame.Data.Demos;

public class SqlInjectionDemo(DatabaseFixture database)
{
    // Typed by an attacker into a "find player by email" field
    private const string Input = "nobody@test.com' OR '1'='1";

    // #region concatenated
    [Fact]
    public void ConcatenatedSql_ReturnsEveryUser()
    {
        using var db = database.CreateContext(new SqlLog());

        var users = db.Users
            .FromSqlRaw("SELECT * FROM users WHERE email = '" + Input + "'")
            .ToList();

        // WHERE email = 'nobody@test.com' OR '1'='1' is true for every row
        Assert.True(users.Count >= DatabaseFixture.UserCount);
    }
    // #endregion concatenated

    // #region parameterized
    [Fact]
    public void InterpolatedFromSql_SendsAParameter()
    {
        using var db = database.CreateContext(new SqlLog());

        // Looks like string interpolation, but FromSql turns every {value} into a parameter
        var users = db.Users
            .FromSql($"SELECT * FROM users WHERE email = {Input}")
            .ToList();

        Assert.Empty(users); // Nobody has the email "nobody@test.com' OR '1'='1"
    }
    // #endregion parameterized
}
