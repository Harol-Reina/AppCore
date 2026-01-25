using System.Runtime.CompilerServices;
using AppCore.Application.Exceptions;
using AppCore.Application.Interfaces;

namespace AppCore.Infrastructure.Services;

/// <summary>
/// Base class for AOT-compatible mapping services that provides common functionality
/// and error handling. Replaces AutoMapper for Native AOT compatibility.
/// </summary>
/// <typeparam name="TSource">The source type to map from</typeparam>
/// <typeparam name="TDestination">The destination type to map to</typeparam>
internal abstract class MappingServiceBase<TSource, TDestination> : IMappingService<TSource, TDestination>
{
    /// <summary>
    /// Maps a single source object to a destination object.
    /// </summary>
    /// <param name="source">The source object to map</param>
    /// <returns>The mapped destination object</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="MappingException">Thrown when mapping fails</exception>
    public TDestination Map(TSource source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        try
        {
            return MapInternal(source);
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new MappingException(
                $"Failed to map from {typeof(TSource).Name} to {typeof(TDestination).Name}",
                ex);
        }
    }

    /// <summary>
    /// Maps a collection of source objects to destination objects.
    /// </summary>
    /// <param name="sources">The collection of source objects to map</param>
    /// <returns>The collection of mapped destination objects</returns>
    /// <exception cref="ArgumentNullException">Thrown when sources is null</exception>
    /// <exception cref="MappingException">Thrown when mapping fails</exception>
    public IEnumerable<TDestination> Map(IEnumerable<TSource> sources)
    {
        if (sources == null)
            throw new ArgumentNullException(nameof(sources));

        return sources.Select(Map);
    }

    /// <summary>
    /// Performs the actual mapping logic. Must be implemented by derived classes
    /// to provide specific mapping behavior for the source and destination types.
    /// </summary>
    /// <param name="source">The source object to map</param>
    /// <returns>The mapped destination object</returns>
    protected abstract TDestination MapInternal(TSource source);
}
