using WordGame.Api;

var builder = WebApplication.CreateBuilder(args);

// #region services
// Everything the game logic depends on is registered here, so tests can replace
// any of it: the clock, the word picker and where the users are stored.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IWordPicker, RandomWordPicker>();
builder.Services.AddSingleton<IGameStore, InMemoryGameStore>();
builder.Services.AddSingleton<IUserRepository>(services =>
    new JsonFileUserRepository(services.GetRequiredService<IConfiguration>()["UsersFile"] ?? "users.json"));
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<GameService>();
// #endregion services

var app = builder.Build();

// Serves the built React app (WordGame.Web builds into wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/games", (StartGameRequest request, GameService games) =>
    games.StartGame(request.PlayerEmail));

app.MapPost("/api/games/complete", (CompleteGameRequest request, GameService games) =>
    games.CompleteGame(request.PlayerEmail, request.Translations));

app.MapGet("/api/users/{email}", (string email, UserService users) =>
    users.GetUserByEmail(email) is { } user ? Results.Ok(user) : Results.NotFound());

app.MapFallbackToFile("index.html");

app.Run();
