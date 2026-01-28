using App.Application.DTOs.Request;
using FluentValidation;

namespace App.Application.Features.Employes.Command;

public class AddEmployeServiceValidator : AbstractValidator<AddEmployeCommand> {
    public AddEmployeServiceValidator() {
        RuleFor(x => x)
            .SetValidator(new EmployeRequestDtoValidator());

    }
}
