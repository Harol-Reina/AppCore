namespace AppCore.Application.Interfaces;

/// <summary>
/// Provides abstraction for date and time operations to support testability.
/// </summary>
public interface IDateTimeService {
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <value>The current date and time in UTC.</value>
    DateTime NowUtc { get; }
    
    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    /// <value>The current local date and time.</value>
    DateTime Now { get; }
}
