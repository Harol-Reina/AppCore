using App.Application.Domain.Entities;
using App.Application.Interfaces;
using App.Application.DTOs.Request;
using OrionSoft.AppCore.Application.Wrappers;
using MediatR;
using OrionSoft.AppCore.Application.Exceptions;

namespace App.Application.Features.Employes.Command;

public record EditEmployeCommand : EmployeRequestDto, IRequest<Response<EmployeEntity>> {
    public int Id { get; set; }
}

public class EditEmployeCommandHandler(IEmployeRepository employeRepository)
                                      : IRequestHandler<EditEmployeCommand, Response<EmployeEntity>> {
    private readonly IEmployeRepository _employeRepository = employeRepository;


    public async Task<Response<EmployeEntity>> Handle(EditEmployeCommand request, CancellationToken cancellationToken) {
        var item = await _employeRepository.GetByIdAsync(request.Id) ??
            throw new NotFoundException(
                $"Employe with id {request.Id} not found"
            );

        item.Name = request.Name;
        item.Email = request.Email;
        item.Phone = request.Phone;
        _ = await _employeRepository.UpdateAsync(item);

        return Response<EmployeEntity>.Success(
            "Finish Ok",
            item
        );
    }
}
