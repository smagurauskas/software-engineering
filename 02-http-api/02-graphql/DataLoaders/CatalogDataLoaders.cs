using GraphQlApi.Models;
using GraphQlApi.Storage;

namespace GraphQlApi.DataLoaders;

public sealed class AuthorByIdDataLoader(
    CatalogFiles files,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options
) : BatchDataLoader<Guid, Author>(batchScheduler, options)
{
    protected override async Task<IReadOnlyDictionary<Guid, Author>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken
    )
    {
        var authors = await files.ReadAuthorsAsync(cancellationToken);

        return authors.Where(author => keys.Contains(author.Id)).ToDictionary(author => author.Id);
    }
}

public sealed class BooksByAuthorIdDataLoader(
    CatalogFiles files,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options
) : GroupedDataLoader<Guid, Book>(batchScheduler, options)
{
    protected override async Task<ILookup<Guid, Book>> LoadGroupedBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken
    )
    {
        var books = await files.ReadBooksAsync(cancellationToken);

        return books.Where(book => keys.Contains(book.AuthorId)).ToLookup(book => book.AuthorId);
    }
}

public sealed class EditionsByBookIdDataLoader(
    CatalogFiles files,
    IBatchScheduler batchScheduler,
    DataLoaderOptions options
) : GroupedDataLoader<Guid, Edition>(batchScheduler, options)
{
    protected override async Task<ILookup<Guid, Edition>> LoadGroupedBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken
    )
    {
        var editions = await files.ReadEditionsAsync(cancellationToken);

        return editions
            .Where(edition => keys.Contains(edition.BookId))
            .ToLookup(edition => edition.BookId);
    }
}
