namespace WordGame.Data.Demos;

public class PaginationDemo(DatabaseFixture database)
{
    private const int PageSize = 20;

    // #region pages
    // Page 900 of the games list, 20 games per page
    [Fact]
    public void Page900_OffsetAndKeyset()
    {
        using var db = database.CreateContext();
        const int page = 900;

        // Skip and Take become OFFSET and LIMIT. Without OrderBy the order of rows is not defined,
        // so the same row could appear on two pages.
        var offsetPage = db.Games.AsNoTracking()
            .OrderBy(g => g.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize);

        // Keyset pagination: the client sends the last id it has seen instead of a page number
        var lastSeenId = db.Games.OrderBy(g => g.Id).Skip((page - 1) * PageSize - 1).Select(g => g.Id).First();
        var keysetPage = db.Games.AsNoTracking()
            .OrderBy(g => g.Id)
            .Where(g => g.Id > lastSeenId)
            .Take(PageSize);

        Assert.Equal(offsetPage.Select(g => g.Id).ToList(), keysetPage.Select(g => g.Id).ToList());

        // OFFSET cannot jump to a row: the database reads all the skipped rows and throws them away
        Assert.Equal(page * PageSize, QueryPlan.RowsRead(QueryPlan.Explain(offsetPage)));
        // The primary key index finds the first row after lastSeenId directly
        Assert.Equal(PageSize, QueryPlan.RowsRead(QueryPlan.Explain(keysetPage)));
    }
    // #endregion pages
}
