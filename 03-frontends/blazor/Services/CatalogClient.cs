using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using CatalogUi.Models;

namespace CatalogUi.Services;

public sealed record RequestEntry(string Label, int Status, int Bytes, long Milliseconds);

public sealed class ApiException(int status, ProblemDetails? problem)
    : Exception(problem?.Title ?? $"The request failed with status {status}.")
{
    public int Status { get; } = status;

    public ProblemDetails? Problem { get; } = problem;
}

public sealed class CatalogClient(HttpClient http)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly List<RequestEntry> history = [];

    public IReadOnlyList<RequestEntry> History => history;

    public void ClearHistory() => history.Clear();

    public Task<List<Author>> GetAuthorsAsync() =>
        SendAsync<List<Author>>(HttpMethod.Get, "/api/authors");

    public Task<List<Book>> GetBooksAsync() =>
        SendAsync<List<Book>>(HttpMethod.Get, "/api/books?sort=-year");

    public Task<List<Book>> GetBooksOfAuthorAsync(Guid authorId) =>
        SendAsync<List<Book>>(HttpMethod.Get, $"/api/authors/{authorId}/books");

    public Task<Book> GetBookAsync(Guid id) => SendAsync<Book>(HttpMethod.Get, $"/api/books/{id}");

    public Task<Author> GetAuthorAsync(Guid id) =>
        SendAsync<Author>(HttpMethod.Get, $"/api/authors/{id}");

    public Task<List<Edition>> GetEditionsAsync(Guid bookId) =>
        SendAsync<List<Edition>>(HttpMethod.Get, $"/api/books/{bookId}/editions");

    public Task<Book> CreateBookAsync(BookCreateRequest request) =>
        SendAsync<Book>(HttpMethod.Post, "/api/books", request);

    public Task<object?> DeleteBookAsync(Guid id) =>
        SendAsync<object?>(HttpMethod.Delete, $"/api/books/{id}");

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null)
    {
        var stopwatch = Stopwatch.StartNew();

        using var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: SerializerOptions);
        }

        using var response = await http.SendAsync(request);

        var text = await response.Content.ReadAsStringAsync();

        history.Add(
            new RequestEntry(
                $"{method} {path}",
                (int)response.StatusCode,
                text.Length,
                stopwatch.ElapsedMilliseconds
            )
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException((int)response.StatusCode, Read<ProblemDetails>(text));
        }

        return string.IsNullOrEmpty(text) ? default! : JsonSerializer.Deserialize<T>(text, SerializerOptions)!;
    }

    private static T? Read<T>(string text)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(text, SerializerOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
