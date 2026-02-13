using OrionSoft.AppCore.Application.Exceptions;

namespace OrionSoft.AppCore.Application.Utils;

internal static class FormatUtils {
    public static Guid ParseGuid(string? input, string parameterName = "Id") {
        if (Guid.TryParse(input, out var parsed))
            return parsed;

        throw new BadRequestException($"Invalid GUID format for {parameterName}: '{input}'");
    }
}
