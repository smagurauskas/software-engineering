using System.Text.Json;

namespace Catalog.Server.Storage;

public sealed record AuthorRecord(Guid Id, string Name, string Country);

public sealed record BookRecord(Guid Id, string Title, Guid AuthorId, int Year);

public sealed record EditionRecord(Guid Id, Guid BookId, string Format, int Pages, int Year);

public sealed class CatalogFiles
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly Lock gate = new();
    private readonly string directory;

    public CatalogFiles(IHostEnvironment environment)
    {
        directory = Path.Combine(environment.ContentRootPath, "data");

        Directory.CreateDirectory(directory);
    }

    public List<AuthorRecord> ReadAuthors() => Read<AuthorRecord>("authors.json");

    public List<BookRecord> ReadBooks() => Read<BookRecord>("books.json");

    public List<EditionRecord> ReadEditions() => Read<EditionRecord>("editions.json");

    public void AddBook(BookRecord book)
    {
        var books = ReadBooks();

        books.Add(book);

        Write("books.json", books);
    }

    private List<T> Read<T>(string fileName)
    {
        lock (gate)
        {
            var path = Path.Combine(directory, fileName);

            if (!File.Exists(path))
            {
                return [];
            }

            using var stream = File.OpenRead(path);

            return JsonSerializer.Deserialize<List<T>>(stream) ?? [];
        }
    }

    private void Write<T>(string fileName, List<T> items)
    {
        lock (gate)
        {
            using var stream = File.Create(Path.Combine(directory, fileName));

            JsonSerializer.Serialize(stream, items, SerializerOptions);
        }
    }

    public void EnsureSeeded()
    {
        var orwell = new AuthorRecord(Guid.Parse("11111111-1111-1111-1111-111111111111"), "George Orwell", "United Kingdom");
        var lem = new AuthorRecord(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Stanislaw Lem", "Poland");

        List<BookRecord> books =
        [
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), "Nineteen Eighty-Four", orwell.Id, 1949),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), "Animal Farm", orwell.Id, 1945),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), "Homage to Catalonia", orwell.Id, 1938),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), "Solaris", lem.Id, 1961),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), "The Cyberiad", lem.Id, 1965),
        ];

        List<EditionRecord> editions = [];

        foreach (var book in books)
        {
            editions.Add(new EditionRecord(Guid.NewGuid(), book.Id, "Hardcover", 240, book.Year));
            editions.Add(new EditionRecord(Guid.NewGuid(), book.Id, "Paperback", 256, book.Year + 3));
        }

        SeedIfEmpty("authors.json", new List<AuthorRecord> { orwell, lem });
        SeedIfEmpty("books.json", books);
        SeedIfEmpty("editions.json", editions);
    }

    private void SeedIfEmpty<T>(string fileName, List<T> items)
    {
        if (IsEmpty<T>(fileName))
        {
            Write(fileName, items);
        }
    }

    private bool IsEmpty<T>(string fileName)
    {
        var path = Path.Combine(directory, fileName);

        if (!File.Exists(path) || new FileInfo(path).Length == 0)
        {
            return true;
        }

        return Read<T>(fileName).Count == 0;
    }
}
