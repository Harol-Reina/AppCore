using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Utils;

namespace App.Application.Common;

public static class AppConstants {
    private static readonly List<string> envErrors = [];

    public static string DefaultConnection { get; }
    public static string SchemaDB { get; }
    public static string PokemonHost { get; } = "https://pokeapi.co/api/v2/";


    static AppConstants() {
        DefaultConnection = GetEnvVariable("DefaultConnection");
        SchemaDB = GetEnvVariable("SchemaDB");
        PokemonHost = GetEnvVariable("PokemonHost");
    }

    private static string GetEnvVariable(string key) {
        var value = Configuration.GetConfig(key);
        if (value is null)
            envErrors.Add(key);
        return value ?? string.Empty;
    }

    public static void Init() {
        if (envErrors.Count > 0)
            throw new OperationException($"Missing required environment variables: {string.Join(", ", envErrors)}");
    }

}
