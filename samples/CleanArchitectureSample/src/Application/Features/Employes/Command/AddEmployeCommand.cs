using App.Application.Domain.Entities;
using App.Application.Interfaces;
using AppCore.Application.Wrappers;
using MediatR;
using App.Application.DTOs.Request;
using App.Application.DTOs.Response;
using App.Application.Common.Mappings;

namespace App.Application.Features.Employes.Command;

public record AddEmployeCommand : EmployeRequestDto, IRequest<Response<EmployeResponseDto>> { }

public class AddEmployeCommandHandler(IEmployeRepository employeRepository)
                                     : IRequestHandler<AddEmployeCommand, Response<EmployeResponseDto>> {
    private readonly IEmployeRepository _employeRepository = employeRepository;

    public async Task<Response<EmployeResponseDto>> Handle(AddEmployeCommand request, CancellationToken cancellationToken) {
        var item = await _employeRepository.AddAsync(request.ToEntity());
        return Response<EmployeResponseDto>.Success(
            "Finish Ok",
            item.ToDto()
        );
    }
}
