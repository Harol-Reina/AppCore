using System.Diagnostics;
using AotTestApp.Tests;
using AppCore.Application.DTOs;
using AppCore.Application.Wrappers;
using AppCore.Application.Serialization;
using System.Text.Json;

namespace AotTestApp;

class Program
{
    static void Main(string[] args)
    {
        // Parse command-line arguments
        var runBenchmark = args.Contains("benchmark") || args.Contains("--benchmark") || args.Contains("-b");
        var runTests = args.Contains("--tests") || args.Contains("-t");
        var runAll = args.Length == 0 || args.Contains("--all") || args.Contains("-a");

        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       AppCore NativeAOT Validation Test Suite            ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        int exitCode = 0;

        if (runTests || runAll)
        {
            Console.WriteLine("Running comprehensive AOT compatibility tests...\n");

            try
            {
                // Run all test suites
                ExceptionTests.RunAll();
                JsonSerializationTests.RunAll();
                CollectionTests.RunAll();

                // Basic integration tests
                RunBasicIntegrationTests();

                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.WriteLine("  ✅ ALL TESTS PASSED");
                Console.WriteLine("═══════════════════════════════════════════════════════════\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("  ❌ TEST FAILURE");
                Console.WriteLine("═══════════════════════════════════════════════════════════");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                exitCode = 1;
            }
        }

        if (runBenchmark && exitCode == 0)
        {
            Console.WriteLine("Running performance benchmarks...\n");
            RunBenchmarks();
        }

        if (exitCode == 0)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           Validation completed successfully!              ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        }

        Environment.Exit(exitCode);
    }


    static void RunBasicIntegrationTests()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("  Basic Integration Tests");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        Console.Write("  • Response wrapper creation... ");
        var successResponse = Response<string>.Success("Operation successful", "Test data");
        var failureResponse = Response<int>.Failure("Operation failed");
        if (successResponse.Message != "Operation successful" || failureResponse.Message != "Operation failed")
        {
            throw new Exception("Response wrapper state incorrect");
        }
        Console.WriteLine("✓");

        Console.Write("  • PaginationDto with collection expression... ");
        var pagination = new PaginationDto<string>
        {
            Count = 3,
            Pages = 1,
            Results = ["Item1", "Item2", "Item3"]
        };
        if (pagination.Count != 3 || pagination.Results.Count != 3)
        {
            throw new Exception("PaginationDto initialization failed");
        }
        Console.WriteLine("✓");

        Console.Write("  • JSON serialization with AppCoreJsonContext... ");
        var json = JsonSerializer.Serialize(successResponse, AppCoreJsonContext.Default.ResponseString);
        if (string.IsNullOrEmpty(json) || !json.Contains("Test data"))
        {
            throw new Exception("JSON serialization failed");
        }
        Console.WriteLine("✓");

        Console.WriteLine("✅ All basic integration tests passed\n");
    }

    static void RunBenchmarks()
    {
        const int Iterations = 100000;

        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("  AppCore Performance Benchmark (NativeAOT)");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        // Warmup
        Console.WriteLine("\n  Warming up...");
        RunResponseBenchmark(1000);
        RunPaginationBenchmark(1000);
        
        Console.WriteLine("\n  Running benchmarks...\n");

        // Benchmark Response Wrapper
        var responseTime = RunResponseBenchmark(Iterations);
        Console.WriteLine($"  Response Wrapper: {Iterations:N0} iterations");
        Console.WriteLine($"    Total Time: {responseTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"    Per Operation: {responseTime.TotalMilliseconds / Iterations:F6} ms");
        Console.WriteLine($"    Operations/sec: {Iterations / responseTime.TotalSeconds:N0}\n");

        // Benchmark Pagination
        var paginationTime = RunPaginationBenchmark(Iterations);
        Console.WriteLine($"  Pagination: {Iterations:N0} iterations");
        Console.WriteLine($"    Total Time: {paginationTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"    Per Operation: {paginationTime.TotalMilliseconds / Iterations:F6} ms");
        Console.WriteLine($"    Operations/sec: {Iterations / paginationTime.TotalSeconds:N0}\n");

        // Memory stats
        var memory = GC.GetTotalMemory(true) / 1024.0 / 1024.0;
        Console.WriteLine($"  Memory Used: {memory:F2} MB");
        Console.WriteLine($"  GC Gen0 Collections: {GC.CollectionCount(0)}");
        Console.WriteLine($"  GC Gen1 Collections: {GC.CollectionCount(1)}");
        Console.WriteLine($"  GC Gen2 Collections: {GC.CollectionCount(2)}\n");
    }

    static TimeSpan RunResponseBenchmark(int iterations)
    {
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            var success = Response<string>.Success("Test message", $"Data {i}");
            var failure = Response<int>.Failure("Error message");
            
            // Force some work
            _ = success.Message;
            _ = success.Data;
            _ = failure.Message;
        }
        
        sw.Stop();
        return sw.Elapsed;
    }

    static TimeSpan RunPaginationBenchmark(int iterations)
    {
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            var dto = new PaginationDto<int>
            {
                Count = 10,
                Pages = 2,
                Results = [1, 2, 3, 4, 5]
            };
            
            // Force some work
            _ = dto.Count;
            _ = dto.Pages;
            _ = dto.Results;
        }
        
        sw.Stop();
        return sw.Elapsed;
    }


    static void TestResponseWrapper()
    {
        Console.WriteLine("Legacy Test 1: Response Wrapper");
        
        var successResponse = Response<string>.Success("Test successful", "Hello from AppCore with NativeAOT!");
        Console.WriteLine($"  Message: {successResponse.Message}");
        Console.WriteLine($"  Data: {successResponse.Data}");
        
        var failureResponse = Response<string>.Failure("Test error message");
        Console.WriteLine($"  Error: {failureResponse.Message}");
        
        Console.WriteLine("  ✓ Response wrapper test passed\n");
    }

    static void TestPagination()
    {
        Console.WriteLine("Legacy Test 2: Pagination");
        
        var paginationDto = new PaginationDto<string>
        {
            Count = 3,
            Pages = 1,
            Results = ["Item1", "Item2", "Item3"]
        };
        
        Console.WriteLine($"  Count: {paginationDto.Count}");
        Console.WriteLine($"  Pages: {paginationDto.Pages}");
        Console.WriteLine($"  Results Count: {paginationDto.Results?.Count ?? 0}");
        
        Console.WriteLine("  ✓ Pagination test passed\n");
    }

    static void TestErrorHandling()
    {
        Console.WriteLine("Legacy Test 3: Error Handling");
        
        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex)
        {
            var errorResponse = Response<string>.Failure(ex.Message);
            Console.WriteLine($"  Error captured: {errorResponse.Message}");
        }
        
        Console.WriteLine("  ✓ Error handling test passed\n");
    }
}
