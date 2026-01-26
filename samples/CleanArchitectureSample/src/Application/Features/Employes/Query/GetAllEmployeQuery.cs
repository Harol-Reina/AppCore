using App.Application.DTOs.Response;
using App.Application.Interfaces;
using AppCore.Application.Exceptions;
using AppCore.Application.Wrappers;
using App.Application.Common.Mappings;
using MediatR;

namespace App.Application.Features.Employes.Query;

public class GetAllEmployeQuery : IRequest<Response<List<EmployeResponseDto>>> { }

public class GetAllEmployeQueryHandler(IEmployeRepository employeRepository)
    : IRequestHandler<GetAllEmployeQuery, Response<List<EmployeResponseDto>>> {
    private readonly IEmployeRepository _employeRepository = employeRepository;

    public async Task<Response<List<EmployeResponseDto>>> Handle(GetAllEmployeQuery request, CancellationToken cancellationToken) {
        var items = await _employeRepository.GetAllAsync() ??
            throw new NotFoundException();
        return Response<List<EmployeResponseDto>>.Success(
            "Finish Ok",
            items.ToDto()
        );
    }
}
