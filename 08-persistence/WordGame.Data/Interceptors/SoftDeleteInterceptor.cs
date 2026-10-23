using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WordGame.Data.Interceptors;

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; set; }
}

// #region soft-delete
// Turns `db.Words.Remove(word)` into setting DeletedAt: the DELETE becomes an UPDATE.
// The query filter in WordGameContext then hides deleted words from every query.
public class SoftDeleteInterceptor(TimeProvider time) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        MarkDeleted(eventData.Context!);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        MarkDeleted(eventData.Context!);
        return ValueTask.FromResult(result);
    }

    private void MarkDeleted(DbContext context)
    {
        var deleted = context.ChangeTracker.Entries<ISoftDeletable>().Where(e => e.State == EntityState.Deleted);
        foreach (var entry in deleted)
        {
            // Unchanged + one changed property: only deleted_at (and updated_at) are written
            entry.State = EntityState.Unchanged;
            entry.Property(nameof(ISoftDeletable.DeletedAt)).CurrentValue = time.GetUtcNow();
        }
    }
}
// #endregion soft-delete
