using System.Net;
using System.Text.Json;
using AppCore.Application.Wrappers;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Wrappers;

public class HttpResponseTests {
    [Fact]
    public void HttpResponse_Constructor_ShouldSetPropertiesCorrectly() {
        // Arrange
        var statusCode = HttpStatusCode.OK;
        var time = 150L;
        var responseJson = JsonDocument.Parse("{ \"test\": \"data\" }");

        // Act
        var httpResponse = new HttpResponse<string>(statusCode, time, responseJson);

        // Assert
        httpResponse.StatusCode.Should().Be(statusCode);
        httpResponse.Time.Should().Be(time);
        httpResponse.Response.Should().Be(responseJson);
    }

    [Fact]
    public void HttpResponse_Constructor_WithoutResponse_ShouldSetDefaultResponse() {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;
        var time = 300L;

        // Act
        var httpResponse = new HttpResponse<string>(statusCode, time);

        // Assert
        httpResponse.StatusCode.Should().Be(statusCode);
        httpResponse.Time.Should().Be(time);
        httpResponse.Response.Should().BeNull();
    }

    [Fact]
    public void HttpResponse_SetData_ShouldUpdateDataProperty() {
        // Arrange
        var httpResponse = new HttpResponse<string>(HttpStatusCode.OK, 100L);
        var testData = "Test response data";

        // Act
        httpResponse.Data = testData;

        // Assert
        httpResponse.Data.Should().Be(testData);
    }

    [Fact]
    public void HttpResponse_SetErrorMessage_ShouldUpdateErrorMessageProperty() {
        // Arrange
        var httpResponse = new HttpResponse<string>(HttpStatusCode.InternalServerError, 500L);
        var errorMessage = "Internal server error occurred";

        // Act
        httpResponse.ErrorMessage = errorMessage;

        // Assert
        httpResponse.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void HttpResponse_ToString_ShouldReturnSerializedJson() {
        // Arrange
        var httpResponse = new HttpResponse<string>(HttpStatusCode.OK, 200L) {
            Data = "Test data",
            ErrorMessage = null
        };

        // Act
        var result = httpResponse.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"statusCode\": 200");
        result.Should().Contain("\"time\": 200");
        result.Should().Contain("\"data\": \"Test data\"");
    }

    [Fact]
    public void HttpResponse_ToString_WithErrorMessage_ShouldIncludeErrorMessage() {
        // Arrange
        var httpResponse = new HttpResponse<object>(HttpStatusCode.BadRequest, 100L) {
            ErrorMessage = "Bad request error"
        };

        // Act
        var result = httpResponse.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"statusCode\": 400");
        result.Should().Contain("\"errorMessage\": \"Bad request error\"");
    }

    [Fact]
    public void HttpResponse_WithComplexData_ShouldSerializeCorrectly() {
        // Arrange
        var complexData = new {
            Id = 123,
            Name = "Test User",
            Items = new[] { "Item1", "Item2" }
        };
        var httpResponse = new HttpResponse<object>(HttpStatusCode.Created, 250L) {
            Data = complexData
        };

        // Act
        var result = httpResponse.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"statusCode\": 201");
        result.Should().Contain("\"time\": 250");
        result.Should().Contain("\"id\": 123");
        result.Should().Contain("\"name\": \"Test User\"");
    }

    [Fact]
    public void HttpResponse_Record_ShouldSupportEquality() {
        // Arrange
        var response1 = new HttpResponse<string>(HttpStatusCode.OK, 100L);
        var response2 = new HttpResponse<string>(HttpStatusCode.OK, 100L);
        var response3 = new HttpResponse<string>(HttpStatusCode.NotFound, 100L);

        // Act & Assert
        response1.Should().Be(response2);
        response1.Should().NotBe(response3);
        response1.GetHashCode().Should().Be(response2.GetHashCode());
    }

    [Fact]
    public void HttpResponse_WithJsonDocument_ShouldHandleComplexResponse() {
        // Arrange
        var jsonData = """
        {
            "users": [
                { "id": 1, "name": "John" },
                { "id": 2, "name": "Jane" }
            ],
            "totalCount": 2
        }
        """;
        var jsonDocument = JsonDocument.Parse(jsonData);
        var httpResponse = new HttpResponse<object>(HttpStatusCode.OK, 180L, jsonDocument);

        // Act
        var result = httpResponse.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("\"statusCode\": 200");
        result.Should().Contain("\"time\": 180");
        httpResponse.Response.Should().NotBeNull();
    }

    [Fact]
    public void HttpResponse_Different_StatusCodes_ShouldWork() {
        // Arrange & Act
        var responses = new[]
        {
            new HttpResponse<string>(HttpStatusCode.OK, 100L),
            new HttpResponse<string>(HttpStatusCode.Created, 150L),
            new HttpResponse<string>(HttpStatusCode.BadRequest, 50L),
            new HttpResponse<string>(HttpStatusCode.Unauthorized, 25L),
            new HttpResponse<string>(HttpStatusCode.InternalServerError, 1000L)
        };

        // Assert
        responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[1].StatusCode.Should().Be(HttpStatusCode.Created);
        responses[2].StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responses[3].StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        responses[4].StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        foreach (var response in responses) {
            response.ToString().Should().NotBeNullOrWhiteSpace();
        }
    }
}
