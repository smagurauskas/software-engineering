using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WordGame.Data.Interceptors;

public interface IAudited
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}

// #region audit
// Runs before every SaveChanges and fills in the timestamps, so no code that saves
// an entity can forget to do it. The clock is injected, so tests can control it.
public class AuditInterceptor(TimeProvider time) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetTimestamps(eventData.Context!);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetTimestamps(eventData.Context!);
        return ValueTask.FromResult(result);
    }

    private void SetTimestamps(DbContext context)
    {
        var now = time.GetUtcNow();
        foreach (var entry in context.ChangeTracker.Entries<IAudited>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
// #endregion audit
