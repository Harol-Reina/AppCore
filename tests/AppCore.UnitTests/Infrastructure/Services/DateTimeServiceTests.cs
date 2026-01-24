using AppCore.Infrastructure.Services;
using AppCore.Application.Interfaces;
using FluentAssertions;
using Xunit;
using System;

namespace AppCore.UnitTests.Infrastructure.Services;

public class DateTimeServiceTests
{
    private readonly IDateTimeService _dateTimeService;

    public DateTimeServiceTests()
    {
        _dateTimeService = new DateTimeService();
    }

    [Fact]
    public void NowUtc_ShouldReturnUtcDateTime()
    {
        // Act
        var result = _dateTimeService.NowUtc;
        var actualUtcNow = DateTime.UtcNow;

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
        // Allow for small timing differences (within 1 second)
        result.Should().BeCloseTo(actualUtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Now_ShouldReturnLocalDateTime()
    {
        // Act
        var result = _dateTimeService.Now;
        var actualNow = DateTime.Now;

        // Assert
        result.Kind.Should().Be(DateTimeKind.Local);
        // Allow for small timing differences (within 1 second)
        result.Should().BeCloseTo(actualNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void NowUtc_MultipleCallsInQuickSuccession_ShouldReturnSimilarTimes()
    {
        // Act
        var time1 = _dateTimeService.NowUtc;
        var time2 = _dateTimeService.NowUtc;

        // Assert
        (time2 - time1).Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void Now_MultipleCallsInQuickSuccession_ShouldReturnSimilarTimes()
    {
        // Act
        var time1 = _dateTimeService.Now;
        var time2 = _dateTimeService.Now;

        // Assert
        (time2 - time1).Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void Service_ShouldImplementIDateTimeService()
    {
        // Assert
        _dateTimeService.Should().BeAssignableTo<IDateTimeService>();
    }
}
