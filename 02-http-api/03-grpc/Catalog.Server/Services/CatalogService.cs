using Catalog.Grpc;
using Catalog.Server.Storage;
using Grpc.Core;

namespace Catalog.Server.Services;

public sealed class CatalogService(CatalogFiles files, ILogger<CatalogService> logger)
    : Catalog.Grpc.Catalog.CatalogBase
{
    private static readonly TimeSpan StreamPace = TimeSpan.FromMilliseconds(300);

    public override Task<Book> GetBook(GetBookRequest request, ServerCallContext context)
    {
        logger.LogInformation("unary GetBook {Id}", request.Id);

        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, $"'{request.Id}' is not a valid id.")
            );
        }

        var book =
            files.ReadBooks().FirstOrDefault(candidate => candidate.Id == id)
            ?? throw new RpcException(
                new Status(StatusCode.NotFound, $"Book {id} does not exist.")
            );

        return Task.FromResult(ToMessage(book, files.ReadAuthors(), files.ReadEditions()));
    }

    public override async Task StreamBooksByAuthor(
        StreamBooksByAuthorRequest request,
        IServerStreamWriter<Book> responseStream,
        ServerCallContext context
    )
    {
        logger.LogInformation("server streaming StreamBooksByAuthor {AuthorId}", request.AuthorId);

        if (!Guid.TryParse(request.AuthorId, out var authorId))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, $"'{request.AuthorId}' is not a valid id.")
            );
        }

        var authors = files.ReadAuthors();

        if (authors.All(author => author.Id != authorId))
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, $"Author {authorId} does not exist.")
            );
        }

        var editions = files.ReadEditions();

        foreach (
            var book in files
                .ReadBooks()
                .Where(book => book.AuthorId == authorId)
                .OrderBy(book => book.Year)
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            await responseStream.WriteAsync(
                ToMessage(book, authors, editions),
                context.CancellationToken
            );
            await Task.Delay(StreamPace, context.CancellationToken);
        }
    }

    public override async Task<ImportSummary> ImportBooks(
        IAsyncStreamReader<NewBook> requestStream,
        ServerCallContext context
    )
    {
        var authors = files.ReadAuthors();
        var summary = new ImportSummary();

        await foreach (var candidate in requestStream.ReadAllAsync(context.CancellationToken))
        {
            logger.LogInformation("client streaming ImportBooks received {Title}", candidate.Title);

            if (string.IsNullOrWhiteSpace(candidate.Title))
            {
                summary.Rejected++;
                summary.Errors.Add("A book without a title was skipped.");

                continue;
            }

            if (
                !Guid.TryParse(candidate.AuthorId, out var authorId)
                || authors.All(author => author.Id != authorId)
            )
            {
                summary.Rejected++;
                summary.Errors.Add($"'{candidate.Title}' references an unknown author.");

                continue;
            }

            files.AddBook(
                new BookRecord(Guid.NewGuid(), candidate.Title.Trim(), authorId, candidate.Year)
            );

            summary.Imported++;
        }

        return summary;
    }

    public override async Task LookupBooks(
        IAsyncStreamReader<GetBookRequest> requestStream,
        IServerStreamWriter<LookupReply> responseStream,
        ServerCallContext context
    )
    {
        var authors = files.ReadAuthors();
        var editions = files.ReadEditions();

        await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
        {
            logger.LogInformation("bidirectional LookupBooks asked for {Id}", request.Id);

            var book = Guid.TryParse(request.Id, out var id)
                ? files.ReadBooks().FirstOrDefault(candidate => candidate.Id == id)
                : null;

            var reply = new LookupReply { RequestedId = request.Id, Found = book is not null };

            if (book is not null)
            {
                reply.Book = ToMessage(book, authors, editions);
            }

            await responseStream.WriteAsync(reply, context.CancellationToken);
        }
    }

    private static Book ToMessage(
        BookRecord book,
        List<AuthorRecord> authors,
        List<EditionRecord> editions
    )
    {
        var author = authors.FirstOrDefault(candidate => candidate.Id == book.AuthorId);

        var message = new Book
        {
            Id = book.Id.ToString(),
            Title = book.Title,
            Year = book.Year,
        };

        if (author is not null)
        {
            message.Author = new Author
            {
                Id = author.Id.ToString(),
                Name = author.Name,
                Country = author.Country,
            };
        }

        message.Editions.AddRange(
            editions
                .Where(edition => edition.BookId == book.Id)
                .Select(edition => new Edition
                {
                    Id = edition.Id.ToString(),
                    Format = edition.Format,
                    Pages = edition.Pages,
                    Year = edition.Year,
                })
        );

        return message;
    }
}
