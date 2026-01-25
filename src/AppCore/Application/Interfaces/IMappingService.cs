namespace AppCore.Application.Interfaces;

/// <summary>
/// Provides AOT-compatible mapping services for converting between different object types.
/// This interface replaces AutoMapper for Native AOT compatibility.
/// </summary>
/// <typeparam name="TSource">The source type to map from</typeparam>
/// <typeparam name="TDestination">The destination type to map to</typeparam>
public interface IMappingService<TSource, TDestination> {
    /// <summary>
    /// Maps a source object to a destination object.
    /// </summary>
    /// <param name="source">The source object to map from</param>
    /// <returns>The mapped destination object</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    TDestination Map(TSource source);

    /// <summary>
    /// Maps a collection of source objects to a collection of destination objects.
    /// </summary>
    /// <param name="sources">The collection of source objects to map from</param>
    /// <returns>The collection of mapped destination objects</returns>
    /// <exception cref="ArgumentNullException">Thrown when sources is null</exception>
    IEnumerable<TDestination> Map(IEnumerable<TSource> sources);
}
