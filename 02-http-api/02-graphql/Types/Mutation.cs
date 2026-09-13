using GraphQlApi.Models;
using GraphQlApi.Storage;

namespace GraphQlApi.Types;

public sealed class Mutation
{
    public async Task<BatchedBook> AddBookAsync(
        string title,
        Guid authorId,
        int year,
        CatalogFiles files,
        CancellationToken cancellationToken
    )
    {
        var authors = await files.ReadAuthorsAsync(cancellationToken);

        if (authors.All(author => author.Id != authorId))
        {
            throw new GraphQLException($"Author {authorId} does not exist.");
        }

        var book = new Book(Guid.NewGuid(), title, authorId, year);

        await files.AppendBookAsync(book, cancellationToken);

        return new BatchedBook(book);
    }

    public async Task<Author> AddAuthorAsync(
        string name,
        string country,
        CatalogFiles files,
        CancellationToken cancellationToken
    )
    {
        var author = new Author(Guid.NewGuid(), name, country);

        await files.AppendAuthorAsync(author, cancellationToken);

        return author;
    }
}
