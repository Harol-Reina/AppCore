using AppCore.Application.DTOs;

namespace AotTestApp.Tests;

/// <summary>
/// Tests C# 14 collection expressions for NativeAOT compatibility.
/// Validates that modern collection syntax compiles and runs correctly in AOT.
/// </summary>
public static class CollectionTests
{
    public static void RunAll()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("  C# 14 Collection Expression AOT Tests");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        TestEmptyCollectionExpression();
        TestCollectionInitializer();
        TestSpreadOperator();
        TestMixedSpreadsAndElements();
        TestPaginationDtoWithCollectionExpressions();
        TestArrayCollectionConversions();

        Console.WriteLine("✅ All collection expression tests passed\n");
    }

    private static void TestEmptyCollectionExpression()
    {
        Console.Write("  • Creating empty collections with []... ");

        try
        {
            // Empty array
            int[] emptyArray = [];
            List<string> emptyList = [];

            if (emptyArray.Length != 0)
            {
                throw new Exception("Empty array has non-zero length");
            }

            if (emptyList.Count != 0)
            {
                throw new Exception("Empty list has non-zero count");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestCollectionInitializer()
    {
        Console.Write("  • Initializing collections with [1, 2, 3]... ");

        try
        {
            int[] numbers = [1, 2, 3, 4, 5];
            List<string> fruits = ["apple", "banana", "cherry"];

            if (numbers.Length != 5 || numbers[0] != 1 || numbers[4] != 5)
            {
                throw new Exception("Number array initialization failed");
            }

            if (fruits.Count != 3 || fruits[0] != "apple" || fruits[2] != "cherry")
            {
                throw new Exception("String list initialization failed");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestSpreadOperator()
    {
        Console.Write("  • Using spread operator [..collection]... ");

        try
        {
            int[] original = [1, 2, 3];
            int[] extended = [0, ..original, 4];

            if (extended.Length != 5)
            {
                throw new Exception($"Expected length 5, got {extended.Length}");
            }

            if (extended[0] != 0 || extended[1] != 1 || extended[4] != 4)
            {
                throw new Exception("Spread operator produced incorrect sequence");
            }

            // Multiple spreads
            int[] first = [1, 2];
            int[] second = [3, 4];
            int[] combined = [..first, ..second];

            if (combined.Length != 4 || combined[2] != 3)
            {
                throw new Exception("Multiple spread operators failed");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestMixedSpreadsAndElements()
    {
        Console.Write("  • Mixing spreads and elements [x, ..arr, y]... ");

        try
        {
            string[] words = ["hello", "world"];
            string[] sentence = ["Start", ..words, "End"];

            if (sentence.Length != 4)
            {
                throw new Exception($"Expected length 4, got {sentence.Length}");
            }

            if (sentence[0] != "Start" || sentence[1] != "hello" || sentence[3] != "End")
            {
                throw new Exception("Mixed spread/element expression failed");
            }

            // Complex mixing
            int[] a = [1, 2];
            int[] b = [5, 6];
            int[] complex = [0, ..a, 3, 4, ..b, 7];

            if (complex.Length != 8 || complex[0] != 0 || complex[7] != 7)
            {
                throw new Exception("Complex mixed expression failed");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestPaginationDtoWithCollectionExpressions()
    {
        Console.Write("  • Using collection expressions in DTOs... ");

        try
        {
            // Create PaginationDto using collection expressions
            var pagination = new PaginationDto<int>
            {
                Count = 10,
                Pages = 1,
                Results = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
            };

            if (pagination.Results == null || pagination.Results.Count != 10)
            {
                throw new Exception("PaginationDto collection expression failed");
            }

            if (pagination.Results[0] != 1 || pagination.Results[9] != 10)
            {
                throw new Exception("PaginationDto collection values incorrect");
            }

            // Empty results
            var emptyPagination = new PaginationDto<string>
            {
                Count = 0,
                Pages = 0,
                Results = []
            };

            if (emptyPagination.Results == null || emptyPagination.Results.Count != 0)
            {
                throw new Exception("Empty PaginationDto collection expression failed");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestArrayCollectionConversions()
    {
        Console.Write("  • Converting between arrays and collections... ");

        try
        {
            // Array to List via collection expression
            int[] sourceArray = [1, 2, 3, 4, 5];
            List<int> list = [..sourceArray];

            if (list.Count != 5 || list[0] != 1 || list[4] != 5)
            {
                throw new Exception("Array to List conversion failed");
            }

            // List to Array via collection expression
            List<string> sourceList = ["A", "B", "C"];
            string[] array = [..sourceList];

            if (array.Length != 3 || array[0] != "A" || array[2] != "C")
            {
                throw new Exception("List to Array conversion failed");
            }

            // Combining different collection types
            int[] arr1 = [1, 2];
            List<int> list1 = [3, 4];
            int[] combined = [..arr1, ..list1, 5];

            if (combined.Length != 5 || combined[4] != 5)
            {
                throw new Exception("Mixed collection type combination failed");
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
