using App.Application.DTOs.Response;
using App.Application.Interfaces;
using AppCore.Application.Exceptions;
using AppCore.Application.Wrappers;
using App.Application.Common.Mappings;
using MediatR;

namespace App.Application.Features.Employes.Query;

public class GetByIdEmployeQuery : IRequest<Response<EmployeResponseDto>> {
    public int Id { get; set; }
}

public class GetByIdEmployeQueryHandler(IEmployeRepository employeRepository)
: IRequestHandler<GetByIdEmployeQuery, Response<EmployeResponseDto>> {
    private readonly IEmployeRepository _employeRepository = employeRepository;
    public async Task<Response<EmployeResponseDto>> Handle(GetByIdEmployeQuery request, CancellationToken cancellationToken) {
        var item = await _employeRepository.GetByIdAsync(request.Id) ??
            throw new NotFoundException($"Employe with Id {request.Id} not found");
        return Response<EmployeResponseDto>.Success(
            "Finish Ok",
            item.ToDto()
        );
    }
}
