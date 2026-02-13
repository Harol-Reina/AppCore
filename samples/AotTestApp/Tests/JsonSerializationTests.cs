using AppCore.Application.Serialization;
using AppCore.Application.Wrappers;
using AppCore.Application.DTOs;
using System.Text.Json;

namespace AotTestApp.Tests;

/// <summary>
/// Tests JSON serialization with AppCoreJsonContext (Source Generator) for NativeAOT.
/// Validates that serialization works without reflection.
/// </summary>
public static class JsonSerializationTests
{
    public static void RunAll()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("  JSON Serialization AOT Tests");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        TestResponseSerialization();
        TestPaginationSerialization();
        TestNestedObjectSerialization();
        TestGenericResponseSerialization();
        TestCollectionSerialization();
        TestRoundTripSerialization();

        Console.WriteLine("✅ All JSON serialization tests passed\n");
    }

    private static void TestResponseSerialization()
    {
        Console.Write("  • Serializing Response<T> with AppCoreJsonContext... ");

        try
        {
            var successResponse = Response<string>.Success("Operation successful", "Test data");
            var failureResponse = Response<int>.Failure("Operation failed");

            // Serialize using AppCoreJsonContext (AOT-compatible) - Explicit TypeInfo
            var successJson = JsonSerializer.Serialize(successResponse, AppCoreJsonContext.Default.ResponseString);
            var failureJson = JsonSerializer.Serialize(failureResponse, AppCoreJsonContext.Default.ResponseInt32);

            if (string.IsNullOrEmpty(successJson) || string.IsNullOrEmpty(failureJson))
            {
                throw new Exception("Serialization produced empty JSON");
            }

            // Verify JSON contains expected data
            if (!successJson.Contains("Test data") || !successJson.Contains("Operation successful"))
            {
                throw new Exception("Success response JSON missing expected data");
            }

            if (!failureJson.Contains("Operation failed"))
            {
                throw new Exception("Failure response JSON missing expected message");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestPaginationSerialization()
    {
        Console.Write("  • Serializing PaginationResponse<T>... ");

        try
        {
            var pagination = new PaginationResponse<string>
            {
                Count = 3,
                Pages = 1,
                Results = ["Item1", "Item2", "Item3"]
            };

            var json = JsonSerializer.Serialize(pagination, AppCoreJsonContext.Default.PaginationResponseString);

            if (string.IsNullOrEmpty(json))
            {
                throw new Exception("Pagination serialization produced empty JSON");
            }

            // Verify collection expression data is serialized
            if (!json.Contains("Item1") || !json.Contains("Item2") || !json.Contains("Item3"))
            {
                throw new Exception("Pagination JSON missing collection items");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestNestedObjectSerialization()
    {
        Console.Write("  • Serializing nested Response<PaginationResponse<T>>... ");

        try
        {
            var pagination = new PaginationResponse<int>
            {
                Count = 5,
                Pages = 2,
                Results = [1, 2, 3, 4, 5]
            };

            var response = Response<PaginationResponse<int>>.Success(
                "Pagination retrieved successfully",
                pagination
            );

            var json = JsonSerializer.Serialize(response, AppCoreJsonContext.Default.ResponsePaginationResponseInt32);

            if (string.IsNullOrEmpty(json))
            {
                throw new Exception("Nested serialization produced empty JSON");
            }

            if (!json.Contains("Pagination retrieved successfully"))
            {
                throw new Exception("Nested JSON missing response message");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestGenericResponseSerialization()
    {
        Console.Write("  • Testing multiple generic types... ");

        try
        {
            var stringResponse = Response<string>.Success("OK", "String data");
            var intResponse = Response<int>.Success("OK", 42);
            var boolResponse = Response<bool>.Success("OK", true);
            var guidResponse = Response<Guid>.Success("OK", Guid.NewGuid());

            var stringJson = JsonSerializer.Serialize(stringResponse, AppCoreJsonContext.Default.ResponseString);
            var intJson = JsonSerializer.Serialize(intResponse, AppCoreJsonContext.Default.ResponseInt32);
            var boolJson = JsonSerializer.Serialize(boolResponse, AppCoreJsonContext.Default.ResponseBoolean);
            var guidJson = JsonSerializer.Serialize(guidResponse, AppCoreJsonContext.Default.ResponseGuid);

            if (string.IsNullOrEmpty(stringJson) ||
                string.IsNullOrEmpty(intJson) ||
                string.IsNullOrEmpty(boolJson) ||
                string.IsNullOrEmpty(guidJson))
            {
                throw new Exception("Generic serialization failed for some types");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestCollectionSerialization()
    {
        Console.Write("  • Testing collection expression serialization... ");

        try
        {
            // Test that C# 14 collection expressions serialize correctly
            var dto = new PaginationResponse<string>
            {
                Count = 5,
                Pages = 1,
                Results = ["Alpha", "Beta", "Gamma", "Delta", "Epsilon"]
            };

            var json = JsonSerializer.Serialize(dto, AppCoreJsonContext.Default.PaginationResponseString);

            // Verify all items from collection expression are present
            if (!json.Contains("Alpha") || !json.Contains("Epsilon"))
            {
                throw new Exception("Collection expression items not serialized");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestRoundTripSerialization()
    {
        Console.Write("  • Testing round-trip serialization (serialize + deserialize)... ");

        try
        {
            var original = Response<string>.Success("Test message", "Original data");

            // Serialize
            var json = JsonSerializer.Serialize(original, AppCoreJsonContext.Default.ResponseString);

            // Deserialize
            var deserialized = JsonSerializer.Deserialize(json, AppCoreJsonContext.Default.ResponseString);

            if (deserialized == null)
            {
                throw new Exception("Deserialization returned null");
            }

            if (deserialized.Message != original.Message)
            {
                throw new Exception($"Message mismatch: '{deserialized.Message}' != '{original.Message}'");
            }

            if (deserialized.Data != original.Data)
            {
                throw new Exception($"Data mismatch: '{deserialized.Data}' != '{original.Data}'");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }
}
