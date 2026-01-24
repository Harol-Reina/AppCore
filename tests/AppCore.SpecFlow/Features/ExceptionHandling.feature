Feature: Exception Handling System
    As a developer using AppCore
    I want to handle exceptions consistently
    So that I can provide meaningful error messages to users

Background:
    Given I am working with the AppCore exception system

Scenario: Creating a custom validation exception
    Given I have invalid input data
    When I create a ValidationException with error details
    Then the exception should have the validation message
    And the exception should be of type ValidationException
    And the exception should inherit from CustomException

Scenario: Creating a not found exception
    Given I am looking for an entity that doesn't exist
    When I create a NotFoundException with entity details
    Then the exception should have a descriptive message
    And the exception should be of type NotFoundException
    And the exception should inherit from CustomException

Scenario: Creating a bad request exception
    Given I receive malformed request data
    When I create a BadRequestException with error details
    Then the exception should have the error message
    And the exception should be of type BadRequestException
    And the exception should inherit from CustomException

Scenario: Creating an authentication exception
    Given I have an authentication failure
    When I create an AuthenticationException with details
    Then the exception should have the authentication message
    And the exception should be of type AuthenticationException
    And the exception should inherit from CustomException

Scenario: Exception hierarchy consistency
    Given I have various AppCore exceptions
    When I check their inheritance chain
    Then all public exceptions should inherit from CustomException
    And CustomException should inherit from Exception
    And all exceptions should maintain proper inheritance hierarchy