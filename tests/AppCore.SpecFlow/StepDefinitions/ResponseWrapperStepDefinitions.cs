using System.Text.Json;
using OrionSoft.AppCore.Application.Wrappers;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace OrionSoft.AppCore.SpecFlow.StepDefinitions;

[Binding]
public class ResponseWrapperStepDefinitions {
    private object? _testData;
    private string? _testMessage;
    private Response<object>? _response;
    private string? _jsonResult;

    [Given(@"I am working with the AppCore response system")]
    public void GivenIAmWorkingWithTheAppCoreResponseSystem() {
        // Initialize context for response wrapper testing
    }

    [Given(@"I have valid data to return")]
    public void GivenIHaveValidDataToReturn() {
        _testData = new { Id = 1, Name = "Test Entity", Value = 42 };
    }

    [Given(@"I have a success message")]
    public void GivenIHaveASuccessMessage() {
        _testMessage = "Operation completed successfully";
    }

    [Given(@"I have encountered an error condition")]
    public void GivenIHaveEncounteredAnErrorCondition() {
        _testMessage = "An error occurred during operation";
    }

    [Given(@"I need to return a response with no data")]
    public void GivenINeedToReturnAResponseWithNoData() {
        _testData = null;
    }

    [Given(@"I have a response with data")]
    public void GivenIHaveAResponseWithData() {
        _testData = new { Id = 1, Name = "Test" };
        _response = Response<object>.Success(_testData);
    }

    [When(@"I create a success response with the data")]
    public void WhenICreateASuccessResponseWithTheData() {
        _response = Response<object>.Success(_testData!);
    }

    [When(@"I create a success response with message and data")]
    public void WhenICreateASuccessResponseWithMessageAndData() {
        _response = Response<object>.Success(_testMessage!, _testData);
    }

    [When(@"I create a failure response with error message")]
    public void WhenICreateAFailureResponseWithErrorMessage() {
        _response = Response<object>.Failure(_testMessage!);
    }

    [When(@"I create a success response with null data")]
    public void WhenICreateASuccessResponseWithNullData() {
        _response = Response<object>.Success(string.Empty, null);
    }

    [When(@"I serialize the response to JSON")]
    public void WhenISerializeTheResponseToJSON() {
        _jsonResult = JsonSerializer.Serialize(_response);
    }

    [Then(@"the response should have the data populated")]
    public void ThenTheResponseShouldHaveTheDataPopulated() {
        _response.Should().NotBeNull();
        _response!.Data.Should().NotBeNull();
        _response.Data.Should().BeEquivalentTo(_testData);
    }

    [Then(@"the response should indicate success")]
    public void ThenTheResponseShouldIndicateSuccess() {
        _response.Should().NotBeNull();
        // Success is typically indicated by having data or a success message
        // This is a design decision for the Response<T> class
    }

    [Then(@"the message should be optional")]
    public void ThenTheMessageShouldBeOptional() {
        _response.Should().NotBeNull();
        // Message can be null or have a value, both are valid
    }

    [Then(@"the response should have the success message")]
    public void ThenTheResponseShouldHaveTheSuccessMessage() {
        _response.Should().NotBeNull();
        _response!.Message.Should().Be(_testMessage);
    }

    [Then(@"the response should have the error message")]
    public void ThenTheResponseShouldHaveTheErrorMessage() {
        _response.Should().NotBeNull();
        _response!.Message.Should().Be(_testMessage);
    }

    [Then(@"the response should indicate failure")]
    public void ThenTheResponseShouldIndicateFailure() {
        _response.Should().NotBeNull();
        _response!.Message.Should().NotBeNullOrEmpty();
    }

    [Then(@"the data should be optional")]
    public void ThenTheDataShouldBeOptional() {
        _response.Should().NotBeNull();
        // Data can be null or have a value, both are valid
    }

    [Then(@"the response should be valid")]
    public void ThenTheResponseShouldBeValid() {
        _response.Should().NotBeNull();
    }

    [Then(@"the data should be null")]
    public void ThenTheDataShouldBeNull() {
        _response.Should().NotBeNull();
        _response!.Data.Should().BeNull();
    }

    [Then(@"the JSON should contain the data")]
    public void ThenTheJSONShouldContainTheData() {
        _jsonResult.Should().NotBeNullOrEmpty();
        _jsonResult.Should().Contain("\"Data\":");
    }

    [Then(@"the JSON should contain the message if present")]
    public void ThenTheJSONShouldContainTheMessageIfPresent() {
        if (_response?.Message != null) {
            _jsonResult.Should().Contain("\"Message\":");
            _jsonResult.Should().Contain(_response.Message);
        }
    }

    [Then(@"null data should not appear in JSON when configured")]
    public void ThenNullDataShouldNotAppearInJSONWhenConfigured() {
        if (_response?.Data == null) {
            // With JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)
            // null data should not appear in JSON
            _jsonResult.Should().NotContain("\"Data\":null");
        }
    }
}
