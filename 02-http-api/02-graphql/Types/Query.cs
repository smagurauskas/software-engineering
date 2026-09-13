using GraphQlApi.Models;
using GraphQlApi.Storage;

namespace GraphQlApi.Types;

public sealed class Query
{
    public async Task<IEnumerable<NaiveBook>> GetBooksNPlusOneAsync(
        CatalogFiles files,
        CancellationToken cancellationToken
    )
    {
        var books = await files.ReadBooksAsync(cancellationToken);

        return books.Select(book => new NaiveBook(book));
    }

    public async Task<IEnumerable<NaiveAuthor>> GetAuthorsNPlusOneAsync(
        CatalogFiles files,
        CancellationToken cancellationToken
    )
    {
        var authors = await files.ReadAuthorsAsync(cancellationToken);

        return authors.Select(author => new NaiveAuthor(author));
    }

    public async Task<IEnumerable<BatchedBook>> GetBooksAsync(
        CatalogFiles files,
        CancellationToken cancellationToken
    )
    {
        var books = await files.ReadBooksAsync(cancellationToken);

        return books.Select(book => new BatchedBook(book));
    }

    public async Task<IEnumerable<Author>> GetAuthorsAsync(
        CatalogFiles files,
        CancellationToken cancellationToken
    ) => await files.ReadAuthorsAsync(cancellationToken);

    public async Task<BatchedBook?> GetBookAsync(
        Guid id,
        CatalogFiles files,
        CancellationToken cancellationToken
    )
    {
        var books = await files.ReadBooksAsync(cancellationToken);
        var book = books.FirstOrDefault(candidate => candidate.Id == id);

        return book is null ? null : new BatchedBook(book);
    }
}
