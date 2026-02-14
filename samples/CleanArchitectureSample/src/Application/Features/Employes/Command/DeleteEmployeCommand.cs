using App.Application.Interfaces;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Wrappers;
using MediatR;

namespace App.Application.Features.Employes.Command;

public class DeleteEmployeCommand : IRequest<Response> {
    public int Id { get; set; }
}

public class DeleteEmployeCommandHandler(IEmployeRepository employeRepository)
: IRequestHandler<DeleteEmployeCommand, Response> {
    private readonly IEmployeRepository _employeRepository = employeRepository;

    public async Task<Response> Handle(DeleteEmployeCommand request, CancellationToken cancellationToken) {
        _ = await _employeRepository.GetByIdAsync(request.Id) ??
            throw new NotFoundException();
        await _employeRepository.DelAsync(request.Id);
        return Response.Success($"Item {request.Id} deleted");
    }
}
