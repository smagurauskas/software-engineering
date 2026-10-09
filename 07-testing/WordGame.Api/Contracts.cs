namespace WordGame.Api;

public record StartGameRequest(string PlayerEmail);

public record StartGameResponse(string[] Words);

public record CompleteGameRequest(string PlayerEmail, string[] Translations);

public record CompleteGameResponse(bool Success, int ExperienceGained);
