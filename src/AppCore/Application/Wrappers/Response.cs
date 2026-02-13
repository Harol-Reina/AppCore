using System.Text.Json.Serialization;

namespace AppCore.Application.Wrappers;

/// <summary>
/// Standard response wrapper for API operations providing consistent structure for success and error responses.
/// </summary>
/// <typeparam name="T">The type of data being wrapped in the response.</typeparam>
/// <example>
/// <code>
/// // Success response with data
/// var success = Response&lt;User&gt;.Success("User found", user);
///
/// // Failure response
/// var failure = Response&lt;User&gt;.Failure("User not found");
/// </code>
/// </example>
public sealed class Response<T> {
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Gets or sets the message describing the operation result.
    /// </summary>
    /// <value>A descriptive message about the operation outcome.</value>
    /// <example>Finish Ok</example>
    public string? Message { get; init; }

    /// <summary>
    /// Gets or sets the data payload of the response.
    /// This property is ignored in JSON serialization when the value is null or default.
    /// </summary>
    /// <value>The actual data being returned by the operation.</value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }

    /// <summary>
    /// Creates a successful response with a message and optional data.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <param name="data">The response data payload.</param>
    /// <returns>A Response instance indicating success.</returns>
    public static Response<T> Success(string message, T? data = default) {
        return new Response<T> { Succeeded = true, Message = message, Data = data };
    }

    /// <summary>
    /// Creates a successful response with data but no explicit message.
    /// </summary>
    /// <param name="data">The response data payload.</param>
    /// <returns>A Response instance indicating success.</returns>
    public static Response<T> Success(T data) {
        return new Response<T> { Succeeded = true, Data = data };
    }

    /// <summary>
    /// Creates a failure response with an error message and optional data.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    /// <param name="data">Optional data to include with the error response.</param>
    /// <returns>A Response instance indicating failure.</returns>
    public static Response<T> Failure(string message, T? data = default) {
        return new Response<T> { Succeeded = false, Message = message, Data = data };
    }
}
