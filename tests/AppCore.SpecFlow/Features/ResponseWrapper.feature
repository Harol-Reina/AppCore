Feature: Response Wrapper Behavior
    As a developer using AppCore
    I want to use standardized response wrappers
    So that I can maintain consistent API responses across applications

Background:
    Given I am working with the AppCore response system

Scenario: Creating a successful response with data
    Given I have valid data to return
    When I create a success response with the data
    Then the response should have the data populated
    And the response should indicate success
    And the message should be optional

Scenario: Creating a successful response with message and data
    Given I have valid data to return
    And I have a success message
    When I create a success response with message and data
    Then the response should have the data populated
    And the response should have the success message
    And the response should indicate success

Scenario: Creating a failure response with error message
    Given I have encountered an error condition
    When I create a failure response with error message
    Then the response should have the error message
    And the response should indicate failure
    And the data should be optional

Scenario: Creating a response with null data
    Given I need to return a response with no data
    When I create a success response with null data
    Then the response should be valid
    And the data should be null
    And the message should be optional

Scenario: Serializing response to JSON
    Given I have a response with data
    When I serialize the response to JSON
    Then the JSON should contain the data
    And the JSON should contain the message if present
    And null data should not appear in JSON when configured