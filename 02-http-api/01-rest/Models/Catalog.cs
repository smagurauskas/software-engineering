namespace RestApi.Models;

public sealed record Author(Guid Id, string Name, string Country);

public sealed record Book(Guid Id, string Title, Guid AuthorId, int Year, string Isbn);

public sealed record AuthorWriteRequest(string Name, string Country);

public sealed record BookCreateRequest(string Title, Guid AuthorId, int Year, string Isbn);

public sealed record BookReplaceRequest(string Title, Guid AuthorId, int Year, string Isbn);

public sealed record BookPatchRequest(string? Title, Guid? AuthorId, int? Year, string? Isbn);
