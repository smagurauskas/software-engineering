using System.Text.Json;

using GraphQlApi.Models;

using Path = System.IO.Path;

namespace GraphQlApi.Storage;

public static class CatalogSeed
{
    public static void EnsureFiles(string directory, JsonSerializerOptions options)
    {
        var orwell = new Author(Guid.Parse("11111111-1111-1111-1111-111111111111"), "George Orwell", "United Kingdom");
        var lem = new Author(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Stanislaw Lem", "Poland");
        var leGuin = new Author(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Ursula K. Le Guin", "United States");

        List<Book> books =
        [
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), "Nineteen Eighty-Four", orwell.Id, 1949),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), "Animal Farm", orwell.Id, 1945),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), "Homage to Catalonia", orwell.Id, 1938),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), "Solaris", lem.Id, 1961),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), "The Cyberiad", lem.Id, 1965),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000006"), "His Master's Voice", lem.Id, 1968),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000007"), "The Left Hand of Darkness", leGuin.Id, 1969),
            new(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000008"), "The Dispossessed", leGuin.Id, 1974),
        ];

        List<Edition> editions = [];

        foreach (var book in books)
        {
            editions.Add(new Edition(Guid.NewGuid(), book.Id, "Hardcover", 240, book.Year));
            editions.Add(new Edition(Guid.NewGuid(), book.Id, "Paperback", 256, book.Year + 3));
        }

        SeedIfEmpty(Path.Combine(directory, "authors.json"), new List<Author> { orwell, lem, leGuin }, options);
        SeedIfEmpty(Path.Combine(directory, "books.json"), books, options);
        SeedIfEmpty(Path.Combine(directory, "editions.json"), editions, options);
    }

    private static void SeedIfEmpty<T>(string path, List<T> items, JsonSerializerOptions options)
    {
        if (!IsEmpty<T>(path))
        {
            return;
        }

        using var stream = File.Create(path);

        JsonSerializer.Serialize(stream, items, options);
    }

    private static bool IsEmpty<T>(string path)
    {
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
        {
            return true;
        }

        using var stream = File.OpenRead(path);

        return (JsonSerializer.Deserialize<List<T>>(stream) ?? []).Count == 0;
    }
}
