using System.Net;
using System.Text.Json;
using OrionSoft.AppCore.Domain.Entities.Integrators;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Domain.Entities.Integrators;

public class HttpAuditEntityTests {
    [Fact]
    public void Constructor_WithRequiredParameters_ShouldSetProperties() {
        // Arrange
        var traceId = Guid.NewGuid();
        var url = "https://api.example.com/test";

        // Act
        var entity = new HttpAuditEntity(traceId, url);

        // Assert
        entity.TraceId.Should().Be(traceId);
        entity.Endpoint.Should().Be(url);
        entity.Method.Should().Be(HttpMethod.Get);
        entity.Headers.Should().BeNull();
        entity.Body.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetProperties() {
        // Arrange
        var traceId = Guid.NewGuid();
        var url = "https://api.example.com/test";
        var method = HttpMethod.Post;
        var body = JsonDocument.Parse("""{"Id": 1, "Name": "Test"}""");
        var headers = JsonDocument.Parse(JsonSerializer.Serialize(new Dictionary<string, string>
        {
            { "Authorization", "Bearer token" },
            { "Content-Type", "application/json" }
        }));

        // Act
        var entity = new HttpAuditEntity(traceId, url, method, body, headers);

        // Assert
        entity.TraceId.Should().Be(traceId);
        entity.Endpoint.Should().Be(url);
        entity.Method.Should().Be(method);
        entity.Headers.Should().NotBeNull();
        entity.Body.Should().NotBeNull();
    }

    [Fact]
    public void StatusCode_ShouldBeSettable() {
        // Arrange
        var entity = new HttpAuditEntity(Guid.NewGuid(), "https://api.example.com/test");

        // Act
        entity.StatusCode = HttpStatusCode.OK;

        // Assert
        entity.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void ElapsedMilliseconds_ShouldBeSettable() {
        // Arrange
        var entity = new HttpAuditEntity(Guid.NewGuid(), "https://api.example.com/test");

        // Act
        entity.ElapsedMilliseconds = 1234;

        // Assert
        entity.ElapsedMilliseconds.Should().Be(1234);
    }

    [Fact]
    public void Response_ShouldBeSettable() {
        // Arrange
        var entity = new HttpAuditEntity(Guid.NewGuid(), "https://api.example.com/test");
        var jsonDoc = System.Text.Json.JsonDocument.Parse("""{"Success": true}""");

        // Act
        entity.Response = jsonDoc;

        // Assert
        entity.Response.Should().NotBeNull();
    }

    [Fact]
    public void InternalError_ShouldBeSettable() {
        // Arrange
        var entity = new HttpAuditEntity(Guid.NewGuid(), "https://api.example.com/test");

        // Act
        entity.InternalError = "Connection timeout";

        // Assert
        entity.InternalError.Should().Be("Connection timeout");
    }

    [Fact]
    public void Constructor_WithNullBody_ShouldSetBodyToNull() {
        // Arrange & Act
        var entity = new HttpAuditEntity(Guid.NewGuid(), "https://api.example.com/test", body: null);

        // Assert
        entity.Body.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullHeaders_ShouldSetHeadersToNull() {
        // Arrange & Act
        var entity = new HttpAuditEntity(Guid.NewGuid(), "https://api.example.com/test", headers: null);

        // Assert
        entity.Headers.Should().BeNull();
    }
}
