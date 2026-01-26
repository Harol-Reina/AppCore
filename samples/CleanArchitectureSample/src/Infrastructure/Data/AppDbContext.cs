using App.Application.Common;
using App.Infrastructure.Data.DAOs;
using AppCore.Infrastructure.Data;
using AppCore.Infrastructure.Data.DAOs.Common;
using AppCore.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;


namespace App.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
: DbContext(options) {

    public DbSet<EmployeDao> Employes { get; private set; } = null!;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.HasDefaultSchema(AppConstants.SchemaDB);
        base.OnModelCreating(modelBuilder);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
            var clrType = entityType.ClrType;
            if (typeof(AuditableBaseDao).IsAssignableFrom(clrType)) {
                modelBuilder.Entity(clrType)
                    .Property(nameof(AuditableBaseDao.CreatedAt))
                    .HasDefaultValueSql("NOW()");
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Development" || env == "Develop" || env == "develop")
            optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }
}
