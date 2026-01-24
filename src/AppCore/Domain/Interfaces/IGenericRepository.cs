using System.Linq.Expressions;
using AppCore.Application.DTOs;
using AppCore.Domain.Common;

namespace AppCore.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio genérico para realizar operaciones CRUD en entidades.
/// </summary>
/// <typeparam name="E">El tipo de la entidad Negocio.</typeparam>
/// <typeparam name="I">El tipo del identificador de la entidad.</typeparam>
public interface IGenericRepository<E, I> where E : BaseEntity<I> {
    /// <summary>
    /// Recupera todas las entidades de forma asincrónica.
    /// </summary>
    /// <param name="includes">Expresiones lambda para incluir navegaciones relacionadas.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene una lista de entidades.</returns>
    Task<List<E>?> GetAllAsync(params Expression<Func<E, object>>[]? includes);

    /// <summary>
    /// Recupera una entidad por su identificador de forma asincrónica.
    /// </summary>
    /// <param name="id">El identificador de la entidad.</param>
    /// <param name="includes">Expresiones lambda para incluir navegaciones relacionadas.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene la entidad si se encuentra; de lo contrario, null.</returns>
    Task<E?> GetByIdAsync(I id, params Expression<Func<E, object>>[]? includes);

    /// <summary>
    /// Agrega una nueva entidad de forma asincrónica.
    /// </summary>
    /// <param name="entity">La entidad a agregar.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene la entidad agregada.</returns>
    Task<E> AddAsync(E entity);

    /// <summary>
    /// Actualiza una entidad existente de forma asincrónica.
    /// </summary>
    /// <param name="entity">La entidad a agregar.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene un booleano que indica si la actualización fue exitosa.</returns>
    Task<E> UpdateAsync(E entity);

    /// <summary>
    /// Elimina una entidad de forma asincrónica.
    /// </summary>
    /// <param name="id">El identificador de la entidad.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene un booleano que indica si la eliminación fue exitosa.</returns>
    Task<bool> DelAsync(I id);

    /// <summary>
    /// Recupera una lista de entidades de forma paginada.
    /// </summary>
    /// <param name="page">El número de página.</param>
    /// <param name="pageSize">El tamaño de la página.</param>
    /// <param name="includes">Expresiones lambda para incluir navegaciones relacionadas.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene una lista de entidades.</returns>
    Task<PaginationDto<E>> GetPagedAsync(int page, int pageSize, params Expression<Func<E, object>>[]? includes);
}
