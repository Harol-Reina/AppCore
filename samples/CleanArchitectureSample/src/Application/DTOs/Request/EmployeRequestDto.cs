using FluentValidation;

namespace App.Application.DTOs.Request;


/// <summary>
/// Represents a request to create or update an employee.
/// </summary>
public record EmployeRequestDto {
    /// <summary>
    /// Gets the name of the employee.
    /// Example: "John Doe"
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the email of the employee.
    /// Example: "john.doe@example.com"
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Gets the phone number of the employee.
    /// Example: "123-456-7890"
    /// </summary>
    public string Phone { get; init; } = null!;

    /// <summary>
    /// Gets the image URL of the employee.
    /// Example: "http://example.com/image.jpg"
    /// </summary>
    public string? Image { get; init; }

};


public class EmployeRequestDtoValidator : AbstractValidator<EmployeRequestDto> {
    public EmployeRequestDtoValidator() {
        RuleFor(x => x.Name)
            .NotNull();
            
        RuleFor(x => x.Email)
            .NotNull();

        RuleFor(x => x.Phone)
            .NotNull()
            .MinimumLength(10);
    }
}
