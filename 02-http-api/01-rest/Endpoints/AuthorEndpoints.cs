using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RestApi.Models;
using RestApi.Storage;

namespace RestApi.Endpoints;

public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this IEndpointRouteBuilder routes)
    {
        var authors = routes.MapGroup("/api/authors").WithTags("Authors");

        authors.MapGet("/", ListAuthors);
        authors.MapGet("/{id:guid}", GetAuthor).WithName("GetAuthor");
        authors.MapPost("/", CreateAuthor);
        authors.MapPut("/{id:guid}", ReplaceAuthor);
        authors.MapPatch("/{id:guid}", PatchAuthor);
        authors.MapDelete("/{id:guid}", DeleteAuthor);

        return authors;
    }

    private static Ok<List<Author>> ListAuthors(
        JsonFileStore store,
        HttpResponse response,
        string? country
    )
    {
        var matches = store
            .ReadAuthors()
            .Where(author =>
                country is null
                || string.Equals(author.Country, country, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(author => author.Name)
            .ToList();

        response.Headers["X-Total-Count"] = matches.Count.ToString();

        return TypedResults.Ok(matches);
    }

    private static Results<Ok<Author>, ProblemHttpResult> GetAuthor(Guid id, JsonFileStore store)
    {
        var author = store.ReadAuthors().SingleOrDefault(candidate => candidate.Id == id);

        return author is null
            ? TypedResults.Problem(
                $"Author {id} does not exist.",
                statusCode: StatusCodes.Status404NotFound
            )
            : TypedResults.Ok(author);
    }

    private static Results<
        CreatedAtRoute<Author>,
        ValidationProblem,
        ProblemHttpResult
    > CreateAuthor(AuthorWriteRequest request, JsonFileStore store)
    {
        if (Validate(request) is { } validationFailure)
        {
            return validationFailure;
        }

        var authors = store.ReadAuthors();

        if (
            authors.Any(author =>
                string.Equals(author.Name, request.Name.Trim(), StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return TypedResults.Problem(
                $"Author {request.Name} already exists.",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        var created = new Author(Guid.NewGuid(), request.Name.Trim(), request.Country.Trim());

        authors.Add(created);
        store.WriteAuthors(authors);

        return TypedResults.CreatedAtRoute(created, "GetAuthor", new { id = created.Id });
    }

    private static Results<Ok<Author>, ValidationProblem, ProblemHttpResult> ReplaceAuthor(
        Guid id,
        AuthorWriteRequest request,
        JsonFileStore store
    )
    {
        if (Validate(request) is { } validationFailure)
        {
            return validationFailure;
        }

        var authors = store.ReadAuthors();
        var index = authors.FindIndex(author => author.Id == id);

        if (index < 0)
        {
            return TypedResults.Problem(
                $"Author {id} does not exist.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        var replaced = new Author(id, request.Name.Trim(), request.Country.Trim());

        authors[index] = replaced;
        store.WriteAuthors(authors);

        return TypedResults.Ok(replaced);
    }

    private static Results<Ok<Author>, ValidationProblem, ProblemHttpResult> PatchAuthor(
        Guid id,
        [FromBody] AuthorPatch patch,
        JsonFileStore store
    )
    {
        var authors = store.ReadAuthors();
        var index = authors.FindIndex(author => author.Id == id);

        if (index < 0)
        {
            return TypedResults.Problem(
                $"Author {id} does not exist.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        var current = authors[index];
        var patched = current with
        {
            Name = patch.Name?.Trim() ?? current.Name,
            Country = patch.Country?.Trim() ?? current.Country,
        };

        if (
            Validate(new AuthorWriteRequest(patched.Name, patched.Country)) is { } validationFailure
        )
        {
            return validationFailure;
        }

        authors[index] = patched;
        store.WriteAuthors(authors);

        return TypedResults.Ok(patched);
    }

    private static Results<NoContent, ProblemHttpResult> DeleteAuthor(Guid id, JsonFileStore store)
    {
        var authors = store.ReadAuthors();

        if (authors.All(author => author.Id != id))
        {
            return TypedResults.Problem(
                $"Author {id} does not exist.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        if (store.ReadBooks().Any(book => book.AuthorId == id))
        {
            return TypedResults.Problem(
                $"Author {id} still has books, delete them first.",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        authors.RemoveAll(author => author.Id == id);
        store.WriteAuthors(authors);

        return TypedResults.NoContent();
    }

    private static ValidationProblem? Validate(AuthorWriteRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(Author.Name)] = ["Name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Country))
        {
            errors[nameof(Author.Country)] = ["Country is required."];
        }

        return errors.Count == 0 ? null : TypedResults.ValidationProblem(errors);
    }
}

public sealed record AuthorPatch(string? Name, string? Country);
