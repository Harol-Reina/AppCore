Feature: HTTP Service Abstractions
    As a developer using AppCore
    I want to make HTTP requests consistently
    So that I can interact with external services reliably

Background:
    Given I am working with the AppCore HTTP service system

Scenario: Making a successful GET request
    Given I have a valid HTTP endpoint
    When I make a GET request to the endpoint
    Then the request should complete successfully
    And I should receive response data
    And the response should have appropriate status code

Scenario: Making a POST request with data
    Given I have a valid HTTP endpoint
    And I have data to send
    When I make a POST request with the data
    Then the request should complete successfully
    And the data should be sent correctly
    And I should receive a response

Scenario: Handling HTTP error responses
    Given I have an HTTP endpoint that returns an error
    When I make a request to the endpoint
    Then I should receive an appropriate exception
    And the exception should contain error details
    And the exception should be properly typed

Scenario: Making requests with authentication
    Given I have an authenticated HTTP service
    And I have valid authentication credentials
    When I make an authenticated request
    Then the request should include authentication headers
    And the request should complete successfully

Scenario: Timeout handling for long-running requests
    Given I have an HTTP endpoint with long response time
    When I make a request with a short timeout
    Then I should receive a timeout exception
    And the request should be cancelled appropriately

Scenario: Retry mechanism for failed requests
    Given I have an HTTP endpoint that fails intermittently
    When I make a request with retry configuration
    Then the request should be retried automatically
    And I should receive a response when the service recovers