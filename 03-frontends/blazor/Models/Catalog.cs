namespace CatalogUi.Models;

public sealed record Author(Guid Id, string Name, string Country);

public sealed record Book(Guid Id, string Title, Guid AuthorId, int Year, string Isbn);

public sealed record Edition(Guid Id, Guid BookId, string Format, int Pages, int Year);

public sealed record BookCreateRequest(string Title, Guid AuthorId, int Year, string Isbn);

public sealed record ProblemDetails(
    string? Title,
    string? Detail,
    int? Status,
    Dictionary<string, string[]>? Errors
);

public sealed record BookDetails(Book Book, Author Author, List<Edition> Editions);
