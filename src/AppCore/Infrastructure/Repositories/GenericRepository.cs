using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using AppCore.Application.DTOs;
using AppCore.Application.Exceptions;
using AppCore.Application.Interfaces;
using AppCore.Domain.Common;
using AppCore.Domain.Interfaces;
using AppCore.Infrastructure.Data.DAOs.Common;
using Microsoft.EntityFrameworkCore;

namespace AppCore.Infrastructure.Repositories;

/// <summary>
/// AOT-compatible generic repository base class that provides common CRUD operations.
/// Uses manual mapping instead of AutoMapper for Native AOT compatibility.
/// </summary>
/// <typeparam name="E">The entity type</typeparam>
/// <typeparam name="I">The ID type</typeparam>
/// <typeparam name="D">The DAO type</typeparam>
[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "D type parameter is annotated with DynamicallyAccessedMembers to preserve properties.")]
public abstract class GenericRepository<E, I, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] D>(
    DbContext dbContext,
    IMappingService<E, D> entityToDao,
    IMappingService<D, E> daoToEntity) : IGenericRepository<E, I>
    where E : BaseEntity<I>
    where D : BaseDao<I> {

    private readonly DbContext _dbContext = dbContext;
    private readonly IMappingService<E, D> _entityToDao = entityToDao;
    private readonly IMappingService<D, E> _daoToEntity = daoToEntity;
    protected DbSet<D> DbSet => _dbContext.Set<D>();

    public virtual async Task<List<E>?> GetAllAsync(params string[]? includes) {
        IQueryable<D> query = DbSet.AsNoTracking(); ;
        try {
            if (includes is not null) {
                foreach (var include in includes) {
                    if (!string.IsNullOrWhiteSpace(include)) {
                        query = query.Include(include);
                    }
                }
            }

            var daos = await query.ToListAsync();
            return _daoToEntity.Map(daos).ToList();
        } catch (DbException ex) {
            throw new ApiDBException(ex);
        }
    }

    public virtual async Task<E?> GetByIdAsync(I id, params string[]? includes) {
        if (id is null) throw new ArgumentNullException(nameof(id));
        try {
            IQueryable<D> query = DbSet.AsNoTracking();
            if (includes is not null) {
                foreach (var include in includes) {
                    if (!string.IsNullOrWhiteSpace(include)) {
                        query = query.Include(include);
                    }
                }
            }
            var dao = await query.FirstOrDefaultAsync(
                x => EqualityComparer<I>.Default.Equals(x.Id, id));
            return dao != null ? _daoToEntity.Map(dao) : null;
        } catch (DbException ex) {
            throw new ApiDBException(ex);
        } catch (Exception ex) {
            throw new ApiDBException(ex);
        }
    }

    public virtual async Task<E> AddAsync(E entity) {
        try {
            var dao = ToDao(entity);
            await DbSet.AddAsync(dao);
            await _dbContext.SaveChangesAsync();
            return ToEntity(dao);
        } catch (DbException ex) {
            throw new ApiDBException(ex);
        } catch (Exception ex) {
            throw new ApiDBException(ex);
        }
    }

    public virtual async Task<E> UpdateAsync(E entity) {
        if (entity.IsNew)
            throw new BadRequestException("Cannot update an entity that hasn't been persisted. Use AddAsync instead.");

        try {
            var dao = ToDao(entity);
            dao.UpdatedAt = DateTime.Now;
            var trackedEntity = DbSet.Local.FirstOrDefault(e =>
                e.Id != null && EqualityComparer<I>.Default.Equals(e.Id, dao.Id));
            if (trackedEntity != null)
                _dbContext.Entry(trackedEntity).State = EntityState.Detached;

            _dbContext.Entry(dao).State = EntityState.Modified;
            DbSet.Update(dao);
            await _dbContext.SaveChangesAsync();
            return ToEntity(dao);
        } catch (Exception ex) {
            throw new ApiDBException(ex);
        }
    }

    public virtual async Task<bool> DelAsync(I id) {
        if (id is null)
            throw new ArgumentNullException(nameof(id), "Cannot delete an entity with null ID.");

        try {
            var dao = await DbSet.FindAsync(id);
            if (dao == null)
                return false;
            DbSet.Remove(dao);
            await _dbContext.SaveChangesAsync(default);
            return true;
        } catch (Exception ex) {
            throw new ApiDBException(ex);
        }
    }

    public virtual async Task<PaginationDto<E>> GetPagedAsync(int page, int pageSize, params string[]? includes) {
        try {
            var skip = (page - 1) * pageSize;
            var query = DbSet.AsQueryable();

            if (includes is not null) {
                foreach (var include in includes) {
                    if (!string.IsNullOrWhiteSpace(include)) {
                        query = query.Include(include);
                    }
                }
            }

            var daos = await query.Skip(skip).Take(pageSize).ToListAsync();
            var totalItems = await query.CountAsync();

            return new PaginationDto<E> {
                Count = totalItems,
                Pages = (int)Math.Ceiling((double)totalItems / pageSize),
                Results = _daoToEntity.Map(daos).OrderBy(r => r.Id).ToList()
            };

        } catch (DbException ex) {
            throw new ApiDBException(ex);
        }
    }

    /// <summary>
    /// Converts an entity to its corresponding DAO using the configured mapping service.
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>The corresponding DAO object</returns>
    protected virtual D ToDao(E entity)
        => _entityToDao.Map(entity);

    /// <summary>
    /// Converts a DAO to its corresponding entity using the configured mapping service.
    /// </summary>
    /// <param name="dao">The DAO to convert</param>
    /// <returns>The corresponding entity object</returns>
    protected virtual E ToEntity(D dao)
        => _daoToEntity.Map(dao);


}

