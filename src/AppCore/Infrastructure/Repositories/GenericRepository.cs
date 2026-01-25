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
internal abstract class GenericRepository<E, I, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] D>(
    DbContext dbContext,
    IMappingService<E, D> entityToDao,
    IMappingService<D, E> daoToEntity) : IGenericRepository<E, I>
    where E : BaseEntity<I>
    where D : BaseDao<I> {

    private readonly DbContext _dbContext = dbContext;
    private readonly IMappingService<E, D> _entityToDao = entityToDao;
    private readonly IMappingService<D, E> _daoToEntity = daoToEntity;
    protected DbSet<D> DbSet => _dbContext.Set<D>();

    public async Task<List<E>?> GetAllAsync(params IEnumerable<Expression<Func<E, object>>>? includes) {
        IQueryable<D> query = DbSet.AsNoTracking(); ;
        try {
            if (includes is not null) {
                foreach (var include in includes) {
                    var daoExpression = ConvertExpression(include);
                    query = query.Include(daoExpression);
                }
            }

            var daos = await query.ToListAsync();
            return _daoToEntity.Map(daos).ToList();
        } catch (DbException ex) {
            throw new ApiDBException(ex);
        }
    }

    public async Task<E?> GetByIdAsync(I id, params IEnumerable<Expression<Func<E, object>>>? includes) {
        if (id is null) throw new ArgumentNullException(nameof(id));
        try {
            IQueryable<D> query = DbSet.AsNoTracking();
            if (includes is not null) {
                foreach (var include in includes) {
                    var daoExpression = ConvertExpression(include);
                    query = query.Include(daoExpression);
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

    public async Task<E> AddAsync(E entity) {
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

    public async Task<E> UpdateAsync(E entity) {
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

    public async Task<bool> DelAsync(I id) {
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

    public async Task<PaginationDto<E>> GetPagedAsync(int page, int pageSize, params IEnumerable<Expression<Func<E, object>>>? includes) {
        try {
            var skip = (page - 1) * pageSize;
            var query = DbSet.AsQueryable();

            if (includes is not null) {
                foreach (var include in includes) {
                    var daoExpression = ConvertExpression(include);
                    query = query.Include(daoExpression);
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

    /// <summary>
    /// Converts an expression for the entity type to an expression for the DAO type.
    /// This is used for Include operations in Entity Framework queries.
    /// AOT-compatible implementation that doesn't rely on AutoMapper reflection.
    /// </summary>
    /// <param name="entityExpression">The entity expression to convert</param>
    /// <returns>The corresponding DAO expression</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is not a member expression</exception>
    [RequiresUnreferencedCode("Expression tree creation may require unreferenced code for member access.")]
    protected virtual Expression<Func<D, object>> ConvertExpression(Expression<Func<E, object>> entityExpression) {
        // Extract property name from entity expression
        var memberExpression = entityExpression.Body is UnaryExpression unary
            ? unary.Operand as MemberExpression
            : entityExpression.Body as MemberExpression;

        if (memberExpression == null)
            throw new ArgumentException("Expression must be a member expression", nameof(entityExpression));

        var propertyName = memberExpression.Member.Name;

        // Create lambda expression for DAO with the same property name
        // This assumes Entity and DAO have matching property names (conventional mapping)
        var parameter = Expression.Parameter(typeof(D), "d");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<D, object>>(conversion, parameter);
    }
}

