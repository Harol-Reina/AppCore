using AppCore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using AppCore.Infrastructure.Data.DAOs.Common;

namespace AppCore.Infrastructure.Data.Interceptors;

public class SaveChangesInterceptor(ICurrentUserService currentUserService,
                                                   IDateTimeService dateTime)
: Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor {
    private readonly ICurrentUserService currentUserService = currentUserService;
    private readonly IDateTimeService dateTime = dateTime;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context) {
        if (context == null) return;
        foreach (var entry in context.ChangeTracker.Entries<AuditableBaseDao>()) {
            if (entry.State == EntityState.Added) {
                entry.Entity.CreatedBy = currentUserService.GetUserName() ?? "System";
                entry.Entity.CreatedAt = dateTime.Now;
            }
            if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities()) {
                entry.Entity.UpdatedBy = currentUserService.GetUserName() ?? "System";
                entry.Entity.UpdatedAt = dateTime.Now;
            }
        }
    }
}

public static class Extensions {
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
