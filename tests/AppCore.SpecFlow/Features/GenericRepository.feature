Feature: Generic Repository Operations
    As a developer using AppCore
    I want to perform CRUD operations using the generic repository
    So that I can manage entities consistently across applications

Background:
    Given I have a configured AppCore context
    And I have a test entity type "TestEntity"

Scenario: Adding a new entity successfully
    Given I have a new entity with valid data
    When I call AddAsync on the repository
    Then the entity should be saved successfully
    And the entity should have an assigned ID
    And the entity should have audit fields populated

Scenario: Retrieving an entity by existing ID
    Given I have an existing entity in the database
    When I call GetByIdAsync with the entity ID
    Then I should receive the correct entity
    And the entity data should match the stored data

Scenario: Updating an existing entity
    Given I have an existing entity in the database
    When I modify the entity data
    And I call UpdateAsync on the repository
    Then the entity should be updated successfully
    And the UpdatedAt field should be current
    And the UpdatedBy field should be populated

Scenario: Deleting an entity successfully
    Given I have an existing entity in the database
    When I call DelAsync with the entity ID
    Then the entity should be removed from the database
    And subsequent GetByIdAsync should return null