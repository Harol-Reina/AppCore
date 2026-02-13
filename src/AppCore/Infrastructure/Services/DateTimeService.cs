using OrionSoft.AppCore.Application.Interfaces;

namespace OrionSoft.AppCore.Infrastructure.Services;

internal sealed class DateTimeService : IDateTimeService {
    public DateTime NowUtc => DateTime.UtcNow;

    public DateTime Now => DateTime.Now;
}
