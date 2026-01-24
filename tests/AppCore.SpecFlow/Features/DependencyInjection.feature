Feature: Dependency Injection Configuration
    As a developer using AppCore
    I want to configure services easily
    So that I can set up my application dependencies quickly

Background:
    Given I am configuring an application with AppCore

Scenario: Registering core AppCore services
    Given I have a service collection
    When I call AddAppCoreCore()
    Then the core interfaces should be registered
    And IGenericRepository should be available
    And ICurrentUserService should be available
    And IDateTimeService should be available

Scenario: Registering services with granular control
    Given I have a service collection
    When I register services individually
    Then I should have full control over service lifetimes
    And I should be able to override default implementations
    And Each service should be independently configurable

Scenario: Service resolution in dependency injection container
    Given I have registered AppCore services
    When I build the service provider
    Then I should be able to resolve IGenericRepository
    And I should be able to resolve ICurrentUserService
    And I should be able to resolve IDateTimeService
    And All services should have appropriate lifetimes

Scenario: Configuration validation
    Given I have registered AppCore services
    When I validate the service configuration
    Then there should be no circular dependencies
    And All required dependencies should be satisfied
    And Service lifetimes should be appropriate

Scenario: Multiple repository registrations
    Given I have multiple entity types
    When I register repositories for each entity type
    Then each repository should be independently resolvable
    And each repository should work with its specific entity type
    And there should be no conflicts between registrations