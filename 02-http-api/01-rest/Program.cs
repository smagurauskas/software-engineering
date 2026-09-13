using RestApi.Endpoints;
using RestApi.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<JsonFileStore>();

var app = builder.Build();

app.Services.GetRequiredService<JsonFileStore>().EnsureSeeded();

app.UseStatusCodePages();

app.MapControllers();
app.MapAuthorEndpoints();

app.MapGet("/", () => Results.Redirect("/api/books"));

app.Run();
