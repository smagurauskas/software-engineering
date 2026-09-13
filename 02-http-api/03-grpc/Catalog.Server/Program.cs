using Catalog.Server.Services;
using Catalog.Server.Storage;

using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
    options.ConfigureEndpointDefaults(endpoint => endpoint.Protocols = HttpProtocols.Http2));

builder.Services.AddGrpc();
builder.Services.AddSingleton<CatalogFiles>();

var app = builder.Build();

app.Services.GetRequiredService<CatalogFiles>().EnsureSeeded();

app.MapGrpcService<CatalogService>();

app.Run();
