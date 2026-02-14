using App.Application.DTOs.Response;
using OrionSoft.AppCore.Domain.Common;
using App.Application.Interfaces;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Wrappers;
using App.Application.Common.Mappings;
using MediatR;

namespace App.Application.Features.Employes.Query;

public class GetAllEmployeQuery : IRequest<Response<PaginationResponse<EmployeResponseDto>>> {
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = "Id";
    public bool Asc { get; set; } = true;
}

public class GetAllEmployeQueryHandler(IEmployeRepository employeRepository)
    : IRequestHandler<GetAllEmployeQuery, Response<PaginationResponse<EmployeResponseDto>>> {
    private readonly IEmployeRepository _employeRepository = employeRepository;

    public async Task<Response<PaginationResponse<EmployeResponseDto>>> Handle(GetAllEmployeQuery request, CancellationToken cancellationToken) {
        var pagedData = await _employeRepository.GetPagedAsync(request.Page, request.PageSize, request.Sort, request.Asc);
        
        var dto = new PaginationResponse<EmployeResponseDto> {
            Count = pagedData.Count,
            Pages = pagedData.Pages,
            Results = pagedData.Results?.Select(e => e.ToDto()).ToList() ?? []
        };

        return Response<PaginationResponse<EmployeResponseDto>>.Success("Finish Ok", dto);
    }
}
