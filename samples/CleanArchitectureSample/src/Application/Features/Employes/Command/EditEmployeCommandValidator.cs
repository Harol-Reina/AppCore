using App.Application.DTOs.Request;
using FluentValidation;

namespace App.Application.Features.Employes.Command;

public class EditEmployeCommandValidator : AbstractValidator<EditEmployeCommand> {
    public EditEmployeCommandValidator() {
        RuleFor(x => x)
            .SetValidator(new EmployeRequestDtoValidator());
    }
}
