using System.Diagnostics;
using Catalog.Grpc;
using Grpc.Core;
using Grpc.Net.Client;
using CatalogClient = Catalog.Grpc.Catalog.CatalogClient;

var address = args.Length > 0 ? args[0] : "http://localhost:5100";

using var channel = GrpcChannel.ForAddress(address);

var client = new CatalogClient(channel);
var clock = Stopwatch.StartNew();

void Log(string message) => Console.WriteLine($"[{clock.ElapsedMilliseconds, 5} ms] {message}");

Console.WriteLine($"talking to {address}");

Console.WriteLine();
Console.WriteLine("1. unary: one request, one response");

var solaris = await client.GetBookAsync(
    new GetBookRequest { Id = "bbbbbbbb-0000-0000-0000-000000000004" }
);

Log(
    $"{solaris.Title} ({solaris.Year}) by {solaris.Author.Name}, {solaris.Editions.Count} editions"
);

try
{
    await client.GetBookAsync(new GetBookRequest { Id = "99999999-9999-9999-9999-999999999999" });
}
catch (RpcException exception)
{
    Log($"status {exception.StatusCode}: {exception.Status.Detail}");
}

Console.WriteLine();
Console.WriteLine("2. server streaming: one request, many responses");

using (
    var call = client.StreamBooksByAuthor(
        new StreamBooksByAuthorRequest { AuthorId = "11111111-1111-1111-1111-111111111111" }
    )
)
{
    await foreach (var book in call.ResponseStream.ReadAllAsync())
    {
        Log($"received {book.Title} ({book.Year})");
    }
}

Console.WriteLine();
Console.WriteLine("3. client streaming: many requests, one response");

using (var call = client.ImportBooks())
{
    NewBook[] candidates =
    [
        new()
        {
            Title = "The Invincible",
            AuthorId = "22222222-2222-2222-2222-222222222222",
            Year = 1964,
        },
        new()
        {
            Title = "Fiasco",
            AuthorId = "22222222-2222-2222-2222-222222222222",
            Year = 1986,
        },
        new()
        {
            Title = "Ghost Book",
            AuthorId = "99999999-9999-9999-9999-999999999999",
            Year = 2024,
        },
        new()
        {
            Title = "",
            AuthorId = "22222222-2222-2222-2222-222222222222",
            Year = 2024,
        },
    ];

    foreach (var candidate in candidates)
    {
        Log($"sending {(string.IsNullOrEmpty(candidate.Title) ? "<no title>" : candidate.Title)}");

        await call.RequestStream.WriteAsync(candidate);
        await Task.Delay(150);
    }

    await call.RequestStream.CompleteAsync();

    var summary = await call.ResponseAsync;

    Log($"imported {summary.Imported}, rejected {summary.Rejected}");

    foreach (var error in summary.Errors)
    {
        Log($"  {error}");
    }
}

Console.WriteLine();
Console.WriteLine("4. bidirectional streaming: both sides talk at the same time");

using (var call = client.LookupBooks())
{
    var reader = Task.Run(async () =>
    {
        await foreach (var reply in call.ResponseStream.ReadAllAsync())
        {
            Log(
                reply.Found
                    ? $"  <- {reply.Book.Title} by {reply.Book.Author.Name}"
                    : $"  <- nothing for {reply.RequestedId}"
            );
        }
    });

    string[] wanted =
    [
        "bbbbbbbb-0000-0000-0000-000000000001",
        "bbbbbbbb-0000-0000-0000-000000000005",
        "99999999-9999-9999-9999-999999999999",
        "bbbbbbbb-0000-0000-0000-000000000003",
    ];

    foreach (var id in wanted)
    {
        Log($"-> asking for {id}");

        await call.RequestStream.WriteAsync(new GetBookRequest { Id = id });
        await Task.Delay(200);
    }

    await call.RequestStream.CompleteAsync();
    await reader;
}

Console.WriteLine();
Console.WriteLine("5. deadlines: the client decides how long it is willing to wait");

try
{
    using var call = client.StreamBooksByAuthor(
        new StreamBooksByAuthorRequest { AuthorId = "11111111-1111-1111-1111-111111111111" },
        deadline: DateTime.UtcNow.AddMilliseconds(500)
    );

    await foreach (var book in call.ResponseStream.ReadAllAsync())
    {
        Log($"received {book.Title}");
    }
}
catch (RpcException exception)
{
    Log($"status {exception.StatusCode}: the server was still streaming when the deadline passed");
}
