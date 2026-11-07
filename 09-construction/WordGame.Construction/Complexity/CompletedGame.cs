using WordGame.Api;

namespace WordGame.Construction.Complexity;

// #region input
// Everything known about a game at the moment the player completes it
public record CompletedGame(
    Game Game,
    DateTimeOffset CompletedAt,
    int HintsUsed,
    int StreakDays,
    bool IsFirstGameToday,
    bool IsPremium);
// #endregion input
