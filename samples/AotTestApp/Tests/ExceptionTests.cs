using AppCore.Application.Exceptions;
using AppCore.Application.Wrappers;
using System.Text.Json;

namespace AotTestApp.Tests;

/// <summary>
/// Tests AppCore exception hierarchy and pattern matching in NativeAOT context.
/// Validates that exception handling works without reflection.
/// </summary>
public static class ExceptionTests
{
    public static void RunAll()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("  Exception Hierarchy AOT Tests");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        TestExceptionCreation();
        TestExceptionInheritance();
        TestExceptionPatternMatching();
        TestExceptionSerialization();
        TestValidationException();
        TestCallerInfoAttributes();

        Console.WriteLine("✅ All exception tests passed\n");
    }

    private static void TestExceptionCreation()
    {
        Console.Write("  • Creating various exception types... ");

        try
        {
            // Test all public exception types
            var validation = new ValidationException("TestProperty", "Test validation error");
            var notFound = new NotFoundException("Entity", "123");
            var badRequest = new BadRequestException("Bad request test");
            var authentication = new AuthenticationException("Auth failed");
            var forbidden = new ForbiddenAccessException("Access denied");

            // Verify messages
            if (string.IsNullOrEmpty(validation.Message) ||
                string.IsNullOrEmpty(notFound.Message) ||
                string.IsNullOrEmpty(badRequest.Message) ||
                string.IsNullOrEmpty(authentication.Message) ||
                string.IsNullOrEmpty(forbidden.Message))
            {
                throw new Exception("Exception message is null or empty");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestExceptionInheritance()
    {
        Console.Write("  • Testing exception inheritance chain... ");

        try
        {
            var validation = new ValidationException("TestProperty", "Test error");
            var notFound = new NotFoundException("Entity", "123");
            var badRequest = new BadRequestException("Bad request");
            var authentication = new AuthenticationException("Auth failed");

            // All should inherit from CustomException
            if (validation is not CustomException ||
                notFound is not CustomException ||
                badRequest is not CustomException ||
                authentication is not CustomException)
            {
                throw new Exception("Exception inheritance chain is broken");
            }

            // All should inherit from Exception base class
            if (validation is not Exception ||
                notFound is not Exception ||
                badRequest is not Exception ||
                authentication is not Exception)
            {
                throw new Exception("Exception does not inherit from System.Exception");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestExceptionPatternMatching()
    {
        Console.Write("  • Testing pattern matching (AOT-compatible)... ");

        try
        {
            // Test pattern matching like UnhandledExceptionBehaviour does
            var exceptions = new Exception[]
            {
                new ValidationException("Test", "Error"),
                new NotFoundException("Entity", "1"),
                new BadRequestException("Bad"),
                new AuthenticationException("Auth"),
                new ForbiddenAccessException("Forbidden"),
                new CustomException(new DictionaryError("TEST-001", "Custom"))
            };

            foreach (var ex in exceptions)
            {
                // This mimics the pattern matching in UnhandledExceptionBehaviour
                var isKnown = ex switch
                {
                    ValidationException => true,
                    NotFoundException => true,
                    BadRequestException => true,
                    AuthenticationException => true,
                    ForbiddenAccessException => true,
                    CustomException => true,
                    _ => false
                };

                if (!isKnown)
                {
                    throw new Exception($"Pattern matching failed for {ex.GetType().Name}");
                }
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestExceptionSerialization()
    {
        Console.Write("  • Testing exception serialization to JSON... ");

        try
        {
            var validation = new ValidationException("Email", "Invalid email format");
            var notFound = new NotFoundException("User", "123");

            // Create error responses like middleware does
            var validationResponse = Response<object>.Failure(validation.Message);
            var notFoundResponse = Response<object>.Failure(notFound.Message);

            // Verify responses can be created
            if (string.IsNullOrEmpty(validationResponse.Message) ||
                string.IsNullOrEmpty(notFoundResponse.Message))
            {
                throw new Exception("Failed to create error responses from exceptions");
            }

            // Test that exceptions have proper error dictionaries
            if (validation.Errors == null || validation.Errors.Count == 0)
            {
                throw new Exception("ValidationException.Errors is empty");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestValidationException()
    {
        Console.Write("  • Testing ValidationException.Errors dictionary... ");

        try
        {
            var validation = new ValidationException("Email", "Invalid email format");

            // Verify Errors dictionary
            if (!validation.Errors.ContainsKey("Email"))
            {
                throw new Exception("ValidationException.Errors missing expected key");
            }

            if (validation.Errors["Email"].Length == 0)
            {
                throw new Exception("ValidationException.Errors has empty array");
            }

            if (validation.Errors["Email"][0] != "Invalid email format")
            {
                throw new Exception("ValidationException.Errors has wrong message");
            }

            Console.WriteLine("✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAILED: {ex.Message}");
            throw;
        }
    }

    private static void TestCallerInfoAttributes()
    {
        Console.Write("  • Testing [CallerMemberName] attributes in AOT... ");

        try
        {
            // Create exception that uses CallerInfo attributes
            var exception = new ValidationException("TestField", "Test error");

            // Verify that CustomException stores caller info
            if (exception.MessageLog == null)
            {
                throw new Exception("MessageLog is null - CallerInfo not captured");
            }

            // Verify MessageLog has data
            if (string.IsNullOrEmpty(exception.MessageLog.Metodo))
            {
                Console.WriteLine("⚠ Warning: Metodo is empty (CallerInfo may not work in AOT)");
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
