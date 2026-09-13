using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using RestApi.Models;
using RestApi.Storage;

namespace RestApi.Controllers;

[ApiController]
[Route("api/books")]
[Produces("application/json")]
public sealed class BooksController(JsonFileStore store) : ControllerBase
{
    [HttpGet(Name = "ListBooks")]
    public ActionResult<IEnumerable<Book>> List(
        [FromQuery] Guid? authorId,
        [FromQuery] int? year,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            return ValidationProblem("page must be >= 1 and pageSize must be between 1 and 100.");
        }

        IEnumerable<Book> books = store.ReadBooks();

        if (authorId is not null)
        {
            books = books.Where(book => book.AuthorId == authorId);
        }

        if (year is not null)
        {
            books = books.Where(book => book.Year == year);
        }

        books = sort switch
        {
            "year" => books.OrderBy(book => book.Year),
            "-year" => books.OrderByDescending(book => book.Year),
            "-title" => books.OrderByDescending(book => book.Title),
            _ => books.OrderBy(book => book.Title),
        };

        var matches = books.ToList();
        var window = matches.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        Response.Headers["X-Total-Count"] = matches.Count.ToString();
        Response.Headers["X-Page"] = page.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(window);
    }

    [HttpGet("{id:guid}", Name = "GetBook")]
    [HttpHead("{id:guid}")]
    public ActionResult<Book> GetById(Guid id)
    {
        var book = store.ReadBooks().SingleOrDefault(candidate => candidate.Id == id);

        if (book is null)
        {
            return NotFoundBook(id);
        }

        var tag = ComputeETag(book);
        Response.Headers.ETag = tag;

        if (Request.Headers.IfNoneMatch.Contains(tag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        return Ok(book);
    }

    [HttpGet("/api/authors/{authorId:guid}/books", Name = "ListBooksOfAuthor")]
    public ActionResult<IEnumerable<Book>> ListByAuthor(Guid authorId)
    {
        if (store.ReadAuthors().All(author => author.Id != authorId))
        {
            return Problem(
                $"Author {authorId} does not exist.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(
            store.ReadBooks().Where(book => book.AuthorId == authorId).OrderBy(book => book.Year)
        );
    }

    [HttpPost]
    public ActionResult<Book> Create([FromBody] BookCreateRequest request)
    {
        if (
            Validate(request.Title, request.AuthorId, request.Year, request.Isbn) is
            { } validationFailure
        )
        {
            return validationFailure;
        }

        var books = store.ReadBooks();

        if (
            books.Any(book =>
                string.Equals(book.Isbn, request.Isbn, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return Problem(
                $"A book with ISBN {request.Isbn} already exists.",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        var created = new Book(
            Guid.NewGuid(),
            request.Title.Trim(),
            request.AuthorId,
            request.Year,
            request.Isbn.Trim()
        );

        books.Add(created);
        store.WriteBooks(books);

        Response.Headers.ETag = ComputeETag(created);

        return CreatedAtRoute("GetBook", new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<Book> Replace(Guid id, [FromBody] BookReplaceRequest request)
    {
        if (
            Validate(request.Title, request.AuthorId, request.Year, request.Isbn) is
            { } validationFailure
        )
        {
            return validationFailure;
        }

        var books = store.ReadBooks();
        var index = books.FindIndex(book => book.Id == id);

        if (index < 0)
        {
            return NotFoundBook(id);
        }

        if (PreconditionFailed(books[index]) is { } preconditionFailure)
        {
            return preconditionFailure;
        }

        var replaced = new Book(
            id,
            request.Title.Trim(),
            request.AuthorId,
            request.Year,
            request.Isbn.Trim()
        );

        books[index] = replaced;
        store.WriteBooks(books);

        Response.Headers.ETag = ComputeETag(replaced);

        return Ok(replaced);
    }

    [HttpPatch("{id:guid}")]
    [Consumes("application/merge-patch+json", "application/json")]
    public ActionResult<Book> Patch(Guid id, [FromBody] BookPatchRequest request)
    {
        var books = store.ReadBooks();
        var index = books.FindIndex(book => book.Id == id);

        if (index < 0)
        {
            return NotFoundBook(id);
        }

        if (PreconditionFailed(books[index]) is { } preconditionFailure)
        {
            return preconditionFailure;
        }

        var current = books[index];

        var patched = current with
        {
            Title = request.Title?.Trim() ?? current.Title,
            AuthorId = request.AuthorId ?? current.AuthorId,
            Year = request.Year ?? current.Year,
            Isbn = request.Isbn?.Trim() ?? current.Isbn,
        };

        if (
            Validate(patched.Title, patched.AuthorId, patched.Year, patched.Isbn) is
            { } patchedFailure
        )
        {
            return patchedFailure;
        }

        books[index] = patched;
        store.WriteBooks(books);

        Response.Headers.ETag = ComputeETag(patched);

        return Ok(patched);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var books = store.ReadBooks();
        var removed = books.RemoveAll(book => book.Id == id);

        if (removed == 0)
        {
            return NotFoundBook(id);
        }

        store.WriteBooks(books);

        return NoContent();
    }

    [HttpOptions]
    [HttpOptions("{id:guid}")]
    public IActionResult Options()
    {
        Response.Headers[HeaderNames.Allow] = "GET, HEAD, POST, PUT, PATCH, DELETE, OPTIONS";

        return NoContent();
    }

    private ActionResult? Validate(string? title, Guid authorId, int year, string? isbn)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(nameof(Book.Title), "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(isbn))
        {
            ModelState.AddModelError(nameof(Book.Isbn), "Isbn is required.");
        }

        if (year is < 1450 or > 2100)
        {
            ModelState.AddModelError(nameof(Book.Year), "Year must be between 1450 and 2100.");
        }

        if (store.ReadAuthors().All(author => author.Id != authorId))
        {
            ModelState.AddModelError(nameof(Book.AuthorId), $"Author {authorId} does not exist.");
        }

        return ModelState.IsValid ? null : ValidationProblem(ModelState);
    }

    private ActionResult? PreconditionFailed(Book current)
    {
        var ifMatch = Request.Headers.IfMatch;

        if (ifMatch.Count == 0 || ifMatch.Contains("*") || ifMatch.Contains(ComputeETag(current)))
        {
            return null;
        }

        return Problem(
            "The book was modified by somebody else, re-read it before writing.",
            statusCode: StatusCodes.Status412PreconditionFailed
        );
    }

    private ActionResult NotFoundBook(Guid id) =>
        Problem($"Book {id} does not exist.", statusCode: StatusCodes.Status404NotFound);

    private static string ComputeETag(Book book)
    {
        var hash = SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(book));

        return $"\"{Convert.ToHexString(hash)[..16].ToLowerInvariant()}\"";
    }
}
