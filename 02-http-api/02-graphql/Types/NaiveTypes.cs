using GraphQlApi.Models;
using GraphQlApi.Storage;

namespace GraphQlApi.Types;

public sealed class NaiveBook(Book book)
{
    public Guid Id => book.Id;

    public string Title => book.Title;

    public int Year => book.Year;

    public async Task<Author?> GetAuthorAsync(CatalogFiles files, CancellationToken cancellationToken)
    {
        var authors = await files.ReadAuthorsAsync(cancellationToken);

        return authors.FirstOrDefault(author => author.Id == book.AuthorId);
    }

    public async Task<List<Edition>> GetEditionsAsync(CatalogFiles files, CancellationToken cancellationToken)
    {
        var editions = await files.ReadEditionsAsync(cancellationToken);

        return editions.Where(edition => edition.BookId == book.Id).ToList();
    }
}

public sealed class NaiveAuthor(Author author)
{
    public Guid Id => author.Id;

    public string Name => author.Name;

    public string Country => author.Country;

    public async Task<List<NaiveBook>> GetBooksAsync(CatalogFiles files, CancellationToken cancellationToken)
    {
        var books = await files.ReadBooksAsync(cancellationToken);

        return books.Where(book => book.AuthorId == author.Id).Select(book => new NaiveBook(book)).ToList();
    }
}
