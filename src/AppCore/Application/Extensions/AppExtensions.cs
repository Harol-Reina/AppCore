using AppCore.Application.Interfaces;
using AppCore.Application.Wrappers;

namespace AppCore.Application.Extensions;

public static class AppExtensions {

    public static Response<T> Success<T>(this Response<T> response, string? message = null, T? data = default)
        => new(message, data);

    public static Response<T> Failure<T>(this Response<T> response, string message)
        => new(message);

}
