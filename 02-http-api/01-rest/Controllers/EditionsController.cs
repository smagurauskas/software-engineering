using Microsoft.AspNetCore.Mvc;
using RestApi.Models;
using RestApi.Storage;

namespace RestApi.Controllers;

[ApiController]
[Route("api/books/{bookId:guid}/editions")]
[Produces("application/json")]
public sealed class EditionsController(JsonFileStore store) : ControllerBase
{
    [HttpGet(Name = "ListEditionsOfBook")]
    public ActionResult<IEnumerable<Edition>> List(Guid bookId)
    {
        if (store.ReadBooks().All(book => book.Id != bookId))
        {
            return Problem($"Book {bookId} does not exist.", statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(store.ReadEditions().Where(edition => edition.BookId == bookId).OrderBy(edition => edition.Year));
    }

    [HttpPost]
    public ActionResult<Edition> Create(Guid bookId, [FromBody] EditionCreateRequest request)
    {
        if (store.ReadBooks().All(book => book.Id != bookId))
        {
            return Problem($"Book {bookId} does not exist.", statusCode: StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrWhiteSpace(request.Format))
        {
            ModelState.AddModelError(nameof(Edition.Format), "Format is required.");
        }

        if (request.Pages is < 1 or > 10000)
        {
            ModelState.AddModelError(nameof(Edition.Pages), "Pages must be between 1 and 10000.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var editions = store.ReadEditions();
        var created = new Edition(Guid.NewGuid(), bookId, request.Format.Trim(), request.Pages, request.Year);

        editions.Add(created);
        store.WriteEditions(editions);

        return CreatedAtRoute("ListEditionsOfBook", new { bookId }, created);
    }
}
