using System.Text.Json;
using RestApi.Models;

namespace RestApi.Storage;

public sealed class JsonFileStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly Lock gate = new();
    private readonly string authorsPath;
    private readonly string booksPath;

    public JsonFileStore(IHostEnvironment environment)
    {
        var directory = Path.Combine(environment.ContentRootPath, "data");
        Directory.CreateDirectory(directory);

        authorsPath = Path.Combine(directory, "authors.json");
        booksPath = Path.Combine(directory, "books.json");
    }

    public List<Author> ReadAuthors() => Read<Author>(authorsPath);

    public List<Book> ReadBooks() => Read<Book>(booksPath);

    public void WriteAuthors(List<Author> authors) => Write(authorsPath, authors);

    public void WriteBooks(List<Book> books) => Write(booksPath, books);

    private List<T> Read<T>(string path)
    {
        lock (gate)
        {
            if (!File.Exists(path))
            {
                return [];
            }

            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize<List<T>>(stream) ?? [];
        }
    }

    private void Write<T>(string path, List<T> items)
    {
        lock (gate)
        {
            var temporaryPath = path + ".tmp";

            using (var stream = File.Create(temporaryPath))
            {
                JsonSerializer.Serialize(stream, items, SerializerOptions);
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
    }

    public void EnsureSeeded()
    {
        var orwell = new Author(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "George Orwell",
            "United Kingdom"
        );
        var lem = new Author(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Stanislaw Lem",
            "Poland"
        );

        SeedIfEmpty(authorsPath, [orwell, lem]);
        SeedIfEmpty(
            booksPath,
            [
                new Book(
                    Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
                    "Nineteen Eighty-Four",
                    orwell.Id,
                    1949,
                    "978-0451524935"
                ),
                new Book(
                    Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"),
                    "Animal Farm",
                    orwell.Id,
                    1945,
                    "978-0452284241"
                ),
                new Book(
                    Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"),
                    "Solaris",
                    lem.Id,
                    1961,
                    "978-0156027601"
                ),
            ]
        );
    }

    private void SeedIfEmpty<T>(string path, List<T> items)
    {
        if (IsEmpty<T>(path))
        {
            Write(path, items);
        }
    }

    private bool IsEmpty<T>(string path)
    {
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
        {
            return true;
        }

        return Read<T>(path).Count == 0;
    }
}
