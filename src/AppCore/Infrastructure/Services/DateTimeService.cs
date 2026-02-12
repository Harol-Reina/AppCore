using AppCore.Application.Interfaces;

namespace AppCore.Infrastructure.Services;

internal sealed class DateTimeService : IDateTimeService {
    public DateTime NowUtc => DateTime.UtcNow;

    public DateTime Now => DateTime.Now;
}
