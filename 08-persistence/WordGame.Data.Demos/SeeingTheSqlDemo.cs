namespace WordGame.Data.Demos;

public class SeeingTheSqlDemo(DatabaseFixture database)
{
    // #region to-query-string
    [Fact]
    public void ToQueryString_ShowsTheSqlWithoutRunningIt()
    {
        using var db = database.CreateContext();

        var query = db.Users
            .Where(u => u.Experience > 500)
            .OrderByDescending(u => u.Experience)
            .Select(u => u.Email);

        var sql = query.ToQueryString();
        Report.WriteLine(sql);

        Assert.Contains("WHERE u.experience > 500", sql);
    }
    // #endregion to-query-string

    // #region parameters
    [Fact]
    public void Variables_BecomeParameters()
    {
        var log = new SqlLog();
        using var db = database.CreateContext(log);

        var email = "player1@test.com";
        var user = db.Users.Single(u => u.Email == email);

        // The constant 500 above was written into the SQL, but a variable is sent as a parameter.
        // The value never becomes part of the SQL text, so it cannot change what the SQL does.
        Assert.Contains("WHERE u.email = @email", log.Commands.Single());
        Assert.DoesNotContain("player1@test.com", log.Commands.Single());
    }
    // #endregion parameters
}
