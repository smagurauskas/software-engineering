namespace WordGame.Data.Demos;

public class LoadingRelatedDataDemo(DatabaseFixture database)
{
    // #region not-included
    [Fact]
    public void Navigation_IsEmptyUnlessIncluded()
    {
        using var db = database.CreateContext();

        var user = db.Users.Single(u => u.Email == "player1@test.com");

        // No error, just wrong data: the player has 20 games, but they were not loaded
        Assert.Empty(user.Games);

        var userWithGames = db.Users
            .Include(u => u.Games)
            .Single(u => u.Email == "player1@test.com");

        Assert.Equal(DatabaseFixture.GamesPerUser, userWithGames.Games.Count);
    }
    // #endregion not-included

    // #region leaderboard
    // The top 100 players with the number of games each of them played
    [Fact]
    public void Leaderboard_ThreeWays()
    {
        var queryPerPlayer = Measure.Run(database, "Query per player (N+1)", db =>
        {
            var players = db.Users.OrderByDescending(u => u.Experience).Take(100).ToList();

            return players
                .Select(p => new LeaderboardRow(p.Email, db.Games.Count(g => g.UserId == p.Id)))
                .ToList();
        });

        var include = Measure.Run(database, "Include", db =>
            db.Users
                .Include(u => u.Games)
                .OrderByDescending(u => u.Experience)
                .Take(100)
                .ToList()
                .Select(p => new LeaderboardRow(p.Email, p.Games.Count))
                .ToList());

        var projection = Measure.Run(database, "Select", db =>
            db.Users
                .OrderByDescending(u => u.Experience)
                .Take(100)
                .Select(p => new LeaderboardRow(p.Email, p.Games.Count))
                .ToList());

        Report.Table(queryPerPlayer, include, projection);

        Assert.Equal(1 + 100, queryPerPlayer.Commands);
        Assert.Equal(1, include.Commands);
        Assert.Equal(1, projection.Commands);
        Assert.True(projection.AllocatedBytes < include.AllocatedBytes);
    }

    private record LeaderboardRow(string Email, int GamesPlayed);
    // #endregion leaderboard
}
