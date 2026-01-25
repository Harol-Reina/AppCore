using System.Diagnostics;
using AppCore.Application.Wrappers;
using AppCore.Application.DTOs;

namespace AotTestApp;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "benchmark")
        {
            RunBenchmarks();
            return;
        }

        Console.WriteLine("AppCore NativeAOT Compatibility Test");
        Console.WriteLine("=====================================\n");

        // Test 1: Response wrapper
        TestResponseWrapper();

        // Test 2: Pagination
        TestPagination();

        // Test 3: Error handling
        TestErrorHandling();

        Console.WriteLine("\n✅ All tests completed successfully!");
        Console.WriteLine("NativeAOT compilation is compatible with AppCore.");
    }

    static void RunBenchmarks()
    {
        const int Iterations = 100000;

        Console.WriteLine("AppCore Performance Benchmark (NativeAOT)");
        Console.WriteLine("=========================================\n");

        // Warmup
        Console.WriteLine("Warming up...");
        RunResponseBenchmark(1000);
        RunPaginationBenchmark(1000);
        
        Console.WriteLine("\nRunning benchmarks...\n");

        // Benchmark Response Wrapper
        var responseTime = RunResponseBenchmark(Iterations);
        Console.WriteLine($"Response Wrapper: {Iterations:N0} iterations");
        Console.WriteLine($"  Total Time: {responseTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  Per Operation: {responseTime.TotalMilliseconds / Iterations:F6} ms");
        Console.WriteLine($"  Operations/sec: {Iterations / responseTime.TotalSeconds:N0}\n");

        // Benchmark Pagination
        var paginationTime = RunPaginationBenchmark(Iterations);
        Console.WriteLine($"Pagination: {Iterations:N0} iterations");
        Console.WriteLine($"  Total Time: {paginationTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  Per Operation: {paginationTime.TotalMilliseconds / Iterations:F6} ms");
        Console.WriteLine($"  Operations/sec: {Iterations / paginationTime.TotalSeconds:N0}\n");

        // Memory stats
        var memory = GC.GetTotalMemory(true) / 1024.0 / 1024.0;
        Console.WriteLine($"Memory Used: {memory:F2} MB");
        Console.WriteLine($"GC Gen0 Collections: {GC.CollectionCount(0)}");
        Console.WriteLine($"GC Gen1 Collections: {GC.CollectionCount(1)}");
        Console.WriteLine($"GC Gen2 Collections: {GC.CollectionCount(2)}");
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
        Console.WriteLine("Test 1: Response Wrapper");
        
        var successResponse = Response<string>.Success("Test successful", "Hello from AppCore with NativeAOT!");
        Console.WriteLine($"  Message: {successResponse.Message}");
        Console.WriteLine($"  Data: {successResponse.Data}");
        
        var failureResponse = Response<string>.Failure("Test error message");
        Console.WriteLine($"  Error: {failureResponse.Message}");
        
        Console.WriteLine("  ✓ Response wrapper test passed\n");
    }

    static void TestPagination()
    {
        Console.WriteLine("Test 2: Pagination");
        
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
        Console.WriteLine("Test 3: Error Handling");
        
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
