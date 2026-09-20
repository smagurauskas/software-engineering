using RestApi.Endpoints;
using RestApi.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<JsonFileStore>();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("ETag", "X-Total-Count", "X-Page", "X-Page-Size")
    )
);

var app = builder.Build();

app.Services.GetRequiredService<JsonFileStore>().EnsureSeeded();

app.UseStatusCodePages();
app.UseCors();

app.MapControllers();
app.MapAuthorEndpoints();

app.MapGet("/", () => Results.Redirect("/api/books"));

app.Run();
