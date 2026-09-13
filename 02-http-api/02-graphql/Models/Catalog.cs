namespace GraphQlApi.Models;

public sealed record Author(Guid Id, string Name, string Country);

public sealed record Book(Guid Id, string Title, Guid AuthorId, int Year);

public sealed record Edition(Guid Id, Guid BookId, string Format, int Pages, int Year);
