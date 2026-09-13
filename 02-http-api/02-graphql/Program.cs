using System.Diagnostics;

using GraphQlApi.DataLoaders;
using GraphQlApi.Storage;
using GraphQlApi.Types;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<CatalogFiles>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<AuthorExtensions>()
    .AddDataLoader<AuthorByIdDataLoader>()
    .AddDataLoader<BooksByAuthorIdDataLoader>()
    .AddDataLoader<EditionsByBookIdDataLoader>();

var app = builder.Build();

app.Services.GetRequiredService<CatalogFiles>().EnsureSeeded();

app.Use(async (context, next) =>
{
    var counter = new ReadCounter();
    var stopwatch = Stopwatch.StartNew();

    context.Items[ReadCounter.ItemKey] = counter;

    await next(context);

    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Diagnostics");

    logger.LogWarning(
        "request finished in {Elapsed} ms with {Reads} file reads",
        stopwatch.ElapsedMilliseconds,
        counter.Value);
});

app.MapGraphQL();

app.Run();
