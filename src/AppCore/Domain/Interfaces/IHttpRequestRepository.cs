using AppCore.Domain.Entities.Integrators;

namespace AppCore.Domain.Interfaces;

public interface IHttpRequestRepository
: IGenericRepository<HttpAuditEntity, int> { }
