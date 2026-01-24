using System.Data.Common;
using System.Linq.Expressions;
using AppCore.Application.DTOs;
using AppCore.Application.Exceptions;
using AppCore.Domain.Common;
using AppCore.Domain.Interfaces;
using AppCore.Infrastructure.Data.DAOs.Common;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppCore.Infrastructure.Repositories;

public abstract class GenericRepository<E, I, D>(DbContext dbContext, IMapper mapper) : IGenericRepository<E, I>
    where E : BaseEntity<I>
    where D : BaseDao<I> {
    private readonly DbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    protected DbSet<D> DbSet => _dbContext.Set<D>();

    public async Task<List<E>?> GetAllAsync(params Expression<Func<E, object>>[]? includes) {
        IQueryable<D> query = DbSet.AsNoTracking();;
        try {
            if (includes is { Length: > 0 }) {
                foreach (var include in includes) {
                    var daoExpression = ConvertExpression(include);
                    query = query.Include(daoExpression);
                }
            }

            var daos = await query.ToListAsync();
            return daos.Select(ToEntity).ToList();
        } catch (DbException ex) {
            throw new ApiDBException(ex);
        }
    }

    public async Task<E?> GetByIdAsync(I id, params Expression<Func<E, object>>[]? includes) {
        if (id is null) throw new ArgumentNullException(nameof(id));
        try {
            IQueryable<D> query = DbSet.AsNoTracking();
            if (includes is { Length: > 0 }) {
                foreach (var include in includes) {
                    var daoExpression = ConvertExpression(include);
                    query = query.Include(daoExpression);
                }
            }
            var dao = await query.FirstOrDefaultAsync(
                x => EqualityComparer<I>.Default.Equals(x.Id, id));
            return dao != null ? ToEntity(dao) : null;
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

    public async Task<PaginationDto<E>> GetPagedAsync(int page, int pageSize, params Expression<Func<E, object>>[]? includes) {
        try {
            var skip = (page - 1) * pageSize;
            var query = DbSet.AsQueryable();

            if (includes is { Length: > 0 }) {
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
                Results = daos.Select(ToEntity).OrderBy(r => r.Id).ToList()
            };

        } catch (DbException ex) {
            throw new ApiDBException(ex);
        }
    }

    protected virtual D ToDao(E entity)
        => _mapper.Map<D>(entity);
    protected virtual E ToEntity(D dao)
        => _mapper.Map<E>(dao);

    /// <summary>
    /// Convierte una expresión de navegación de la entidad (E) a la entidad DAO (D).
    /// Utiliza AutoMapper para mapear los nombres de propiedades.
    /// </summary>
    protected virtual Expression<Func<D, object>> ConvertExpression(Expression<Func<E, object>> entityExpression) {
        // Extraer el nombre de la propiedad de la expresión de la entidad
        var memberExpression = entityExpression.Body is UnaryExpression unary
            ? unary.Operand as MemberExpression
            : entityExpression.Body as MemberExpression;

        if (memberExpression == null)
            throw new ArgumentException("Expression must be a member expression", nameof(entityExpression));

        var propertyName = memberExpression.Member.Name;

        // Crear una expresión lambda para el DAO con el mismo nombre de propiedad
        var parameter = Expression.Parameter(typeof(D), "d");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<D, object>>(conversion, parameter);
    }
}

