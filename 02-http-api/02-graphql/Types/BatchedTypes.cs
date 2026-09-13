using GraphQlApi.DataLoaders;
using GraphQlApi.Models;

using HotChocolate.Types;

namespace GraphQlApi.Types;

public sealed class BatchedBook(Book book)
{
    public Guid Id => book.Id;

    public string Title => book.Title;

    public int Year => book.Year;

    public async Task<Author?> GetAuthorAsync(
        AuthorByIdDataLoader authorById,
        CancellationToken cancellationToken) =>
        await authorById.LoadAsync(book.AuthorId, cancellationToken);

    public async Task<IEnumerable<Edition>> GetEditionsAsync(
        EditionsByBookIdDataLoader editionsByBookId,
        CancellationToken cancellationToken) =>
        await editionsByBookId.LoadAsync(book.Id, cancellationToken) ?? [];
}

[ExtendObjectType<Author>]
public sealed class AuthorExtensions
{
    public async Task<IEnumerable<BatchedBook>> GetBooksAsync(
        [Parent] Author author,
        BooksByAuthorIdDataLoader booksByAuthorId,
        CancellationToken cancellationToken)
    {
        var books = await booksByAuthorId.LoadAsync(author.Id, cancellationToken) ?? [];

        return books.Select(book => new BatchedBook(book));
    }
}
