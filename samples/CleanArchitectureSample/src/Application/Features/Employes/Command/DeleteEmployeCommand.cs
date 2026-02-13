using App.Application.Interfaces;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Wrappers;
using MediatR;

namespace App.Application.Features.Employes.Command;

public class DeleteEmployeCommand : IRequest<Response<bool>> {
    public int Id { get; set; }
}

public class DeleteEmployeCommandHandler(IEmployeRepository employeRepository)
: IRequestHandler<DeleteEmployeCommand, Response<bool>> {
    private readonly IEmployeRepository _employeRepository = employeRepository;

    public async Task<Response<bool>> Handle(DeleteEmployeCommand request, CancellationToken cancellationToken) {
        _ = await _employeRepository.GetByIdAsync(request.Id) ??
            throw new NotFoundException();
        await _employeRepository.DelAsync(request.Id);
        return Response<bool>.Success($"Item {request.Id} deleted");
    }
}
