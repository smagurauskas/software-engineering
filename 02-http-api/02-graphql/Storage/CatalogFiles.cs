using System.Text.Json;

using GraphQlApi.Models;

using Path = System.IO.Path;

namespace GraphQlApi.Storage;

public sealed class CatalogFiles
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
    private static readonly TimeSpan DiskLatency = TimeSpan.FromMilliseconds(50);

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string directory;
    private readonly ILogger<CatalogFiles> logger;
    private readonly IHttpContextAccessor httpContextAccessor;

    public CatalogFiles(
        IHostEnvironment environment,
        ILogger<CatalogFiles> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        this.logger = logger;
        this.httpContextAccessor = httpContextAccessor;

        directory = Path.Combine(environment.ContentRootPath, "data");
        Directory.CreateDirectory(directory);
    }

    public void EnsureSeeded() => CatalogSeed.EnsureFiles(directory, SerializerOptions);

    public Task<List<Author>> ReadAuthorsAsync(CancellationToken cancellationToken) =>
        ReadAsync<Author>("authors.json", cancellationToken);

    public Task<List<Book>> ReadBooksAsync(CancellationToken cancellationToken) =>
        ReadAsync<Book>("books.json", cancellationToken);

    public Task<List<Edition>> ReadEditionsAsync(CancellationToken cancellationToken) =>
        ReadAsync<Edition>("editions.json", cancellationToken);

    public async Task AppendBookAsync(Book book, CancellationToken cancellationToken)
    {
        var books = await ReadBooksAsync(cancellationToken);

        books.Add(book);

        await WriteAsync("books.json", books, cancellationToken);
    }

    public async Task<bool> RemoveBookAsync(Guid id, CancellationToken cancellationToken)
    {
        var books = await ReadBooksAsync(cancellationToken);

        if (books.RemoveAll(book => book.Id == id) == 0)
        {
            return false;
        }

        await WriteAsync("books.json", books, cancellationToken);

        return true;
    }

    public async Task AppendAuthorAsync(Author author, CancellationToken cancellationToken)
    {
        var authors = await ReadAuthorsAsync(cancellationToken);

        authors.Add(author);

        await WriteAsync("authors.json", authors, cancellationToken);
    }

    private async Task<List<T>> ReadAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var counter = httpContextAccessor.HttpContext?.Items[ReadCounter.ItemKey] as ReadCounter;

        logger.LogInformation("file read #{Count} in this request: {FileName}", counter?.Increment() ?? 0, fileName);

        await gate.WaitAsync(cancellationToken);

        try
        {
            await Task.Delay(DiskLatency, cancellationToken);

            await using var stream = File.OpenRead(Path.Combine(directory, fileName));

            return await JsonSerializer.DeserializeAsync<List<T>>(stream, cancellationToken: cancellationToken) ?? [];
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task WriteAsync<T>(string fileName, List<T> items, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(Path.Combine(directory, fileName));

        await JsonSerializer.SerializeAsync(stream, items, SerializerOptions, cancellationToken);
    }
}
