using OrionSoft.AppCore.Domain.Entities.Integrators;

namespace OrionSoft.AppCore.Domain.Interfaces;

public interface IHttpRequestRepository
: IGenericRepository<HttpAuditEntity, int> { }
