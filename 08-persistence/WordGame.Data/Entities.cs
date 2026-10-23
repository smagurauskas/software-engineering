using System.ComponentModel.DataAnnotations;
using WordGame.Data.Interceptors;

namespace WordGame.Data;

// #region entities
// Plain classes: EF maps them to tables by convention. `Id` becomes the primary key,
// `UserId` + `User` become a foreign key, and List<> properties become relationships.
public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public int Experience { get; set; }

    public List<Game> Games { get; set; } = [];
}

public class Game
{
    public int Id { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public int Experience { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Many-to-many: EF creates the game_word join table itself
    public List<Word> Words { get; set; } = [];
}

// Words are managed by admins, so changes are audited, and removed words are only
// marked as deleted: old games still have to show which words they had.
public class Word : IAudited, ISoftDeletable
{
    public int Id { get; set; }
    public required string English { get; set; }
    public required string German { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Mapped to PostgreSQL's xmin system column, which changes on every update.
    // EF adds it to the WHERE of every UPDATE to detect concurrent edits.
    [Timestamp]
    public uint Version { get; set; }
}
// #endregion entities
