using AppCore.Application.Interfaces;
using AppCore.Infrastructure.Data.DAOs.Common;

namespace AppCore.Infrastructure.Extensions;

public static class AuditableExtensions {
    public static void SetAuditCreate(this IAuditableBaseDao entity, ICurrentUserService userService) {
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = userService.GetUserName();
        entity.UpdatedAt = null;
        entity.UpdatedBy = null;
    }

    public static void SetAuditUpdate(this IAuditableBaseDao entity, ICurrentUserService userService) {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userService.GetUserName();
    }
}
