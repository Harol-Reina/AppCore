# Contributing to AppCore

Thank you for your interest in contributing to AppCore! We welcome contributions from everyone.

## 🚀 Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Setting up the Development Environment

1. **Fork the repository**
   ```bash
   git clone https://github.com/your-username/AppCore.git
   cd AppCore
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

## 📋 Development Workflow

### Branch Naming Convention

- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `hotfix/description` - Critical fixes
- `docs/description` - Documentation changes
- `refactor/description` - Code refactoring

### Commit Policy with Explicit Approval

#### Core Principle

> **AI assistants never execute `git commit` automatically.**

AI can only **propose commits**. Commits are executed **only** after explicit user approval.

#### Standard Workflow (Commit Approval Flow)

1. **Implementation of Changes**
   - AI implements the requested changes according to the specification
   - Changes remain in *working tree* or *staged* state, but **without commit**

2. **Stage & Summary**
   AI presents:
   - List of modified files
   - Summary of change intention
   - Relevant risks or impacts

3. **Commit Proposal**
   For each proposed commit, AI must show:
   - Proposed commit identifier (Commit 1, Commit 2, etc.)
   - Message in **Conventional Commits** format
   - Functional scope of the commit (what it includes)
   - Explicit exclusions (what it does NOT include)

4. **Approval Gate**
   The flow **stops** until receiving explicit user instruction.
   
   Valid user commands:
   - `APPROVE COMMIT <n>` → Execute only the indicated commit
   - `APPROVE ALL` → Execute all proposed commits
   - `CHANGE: <instructions>` → Request adjustments before approving
   - `CANCEL` → Discard proposed changes

5. **Commit Execution**
   - Only after explicit approval
   - Commit is executed locally
   - Generated hash is reported

6. **Post-Commit**
   - Update documentation or metrics if applicable
   - Continue with next micro-sprint

#### Commit Registry

**New Format (MANDATORY):**

**Proposed commits (pending approval):**
1. `<commit message>` — `<files/area>` — `<objective>`

**Approved commits (executed):**
1. `<commit message>` — `<hash>`

#### Commit Size and Frequency Policy

- 1 commit = 1 clear intention
- Maximum recommended:
  - 1–5 files per commit
  - Exceptionally more, only in mechanical refactors
- Documentation and code:
  - Prefer separate commits (`feat/fix` and then `docs:`)

#### Micro-Sprint Strategy

- Replace "1 full week" with **0.5–1 day micro-sprints**
- Each micro-sprint must produce:
  - Verifiable changes
  - Passing tests
  - 1–3 proposed commits (maximum)
- The week is considered a **container of micro-sprints**, not a single execution

#### Template for AI Requests

**Context:**
- Change objective
- Technical constraints

**Specification:**
- Acceptance criteria
- Allowed files/areas
- Forbidden files/areas

**Expected commit plan:**
- Maximum number of commits
- Rule: only propose, do not execute

**Required output:**
- List of modified files
- Summary of changes
- Commit proposals

#### Golden Rule

> ❝If the user didn't write APPROVE, the commit doesn't exist❞

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add new exception handling mechanism
fix: resolve null reference in generic repository
docs: update API documentation
test: add unit tests for response wrapper
refactor: simplify dependency injection setup
```

**Commit Size Guidelines:**
- 1 commit = 1 clear intention
- Maximum: 1-5 files per commit
- Separate documentation from code changes

### Development Process

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow existing code style
   - Add tests for new functionality
   - Update documentation if needed

3. **Run quality checks**
   ```bash
   # Run tests
   dotnet test
   
   # Run BDD specifications
   dotnet test tests/AppCore.SpecFlow
   
   # Check code coverage
   dotnet test --collect:"XPlat Code Coverage"
   ```

4. **Stage and review your changes**
   ```bash
   git add <specific-files>
   git status
   git diff --staged
   ```

5. **Commit with approval**
   - If working with AI: Wait for commit proposal and approve explicitly
   - If working manually:
     ```bash
     git commit -m "feat: your descriptive commit message"
     ```

6. **Push and create PR**
   ```bash
   git push origin feature/your-feature-name
   ```

## 🧪 Testing Guidelines

### Unit Tests

- Place unit tests in `tests/AppCore.UnitTests/`
- Follow the AAA pattern (Arrange, Act, Assert)
- Use FluentAssertions for readable assertions
- Mock dependencies using Moq

```csharp
[Fact]
public async Task Should_Return_Success_Response_When_Entity_Found()
{
    // Arrange
    var mockRepository = new Mock<IGenericRepository<User, int>>();
    var expectedUser = new User { Id = 1, Name = "Test" };
    mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedUser);
    
    var service = new UserService(mockRepository.Object);
    
    // Act
    var result = await service.GetUserAsync(1);
    
    // Assert
    result.Should().NotBeNull();
    result.Data.Should().Be(expectedUser);
}
```

### BDD Tests (SpecFlow)

- Place SpecFlow tests in `tests/AppCore.SpecFlow/`
- Write scenarios in Gherkin syntax
- Implement step definitions in `StepDefinitions/`

```gherkin
Feature: Generic Repository Operations
  As a developer
  I want to perform CRUD operations
  So that I can manage entities effectively

Scenario: Successfully retrieve an entity by ID
  Given I have a generic repository for User entities
  And a user exists with ID 1
  When I request the user with ID 1
  Then I should receive the user data
  And the response should be successful
```

### Integration Tests

- Place integration tests in `tests/AppCore.IntegrationTests/`
- Test the full stack with real dependencies
- Use TestContainers for database testing

## 📝 Code Style

### C# Conventions

- Use PascalCase for public members
- Use camelCase for private fields
- Use `_` prefix for private fields
- Follow Microsoft naming conventions
- Maximum line length: 120 characters

### Documentation

- Add XML documentation for all public APIs
- Include examples in documentation
- Update README.md for significant changes
- Add inline comments for complex logic

```csharp
/// <summary>
/// Retrieves an entity by its unique identifier.
/// </summary>
/// <typeparam name="E">The entity type</typeparam>
/// <typeparam name="I">The identifier type</typeparam>
/// <param name="id">The unique identifier of the entity</param>
/// <returns>The entity if found, null otherwise</returns>
/// <example>
/// <code>
/// var user = await repository.GetByIdAsync(1);
/// </code>
/// </example>
public async Task<E?> GetByIdAsync(I id)
```

## 🔍 Code Review Process

### Before Submitting PR

- [ ] All tests pass
- [ ] Code coverage meets minimum threshold (80%)
- [ ] Documentation is updated
- [ ] Breaking changes are documented
- [ ] Performance impact is considered

### PR Requirements

- Clear description of changes
- Link to related issues
- Screenshots for UI changes
- Breaking changes clearly marked
- Migration instructions if needed

### Review Checklist

- [ ] Code follows project conventions
- [ ] Tests are comprehensive
- [ ] Documentation is accurate
- [ ] Performance is acceptable
- [ ] Security considerations addressed

## 🐛 Bug Reports

### Before Creating an Issue

1. Check existing issues for duplicates
2. Verify the bug with the latest version
3. Create a minimal reproduction case

### Bug Report Template

```markdown
**Describe the bug**
A clear description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

**Expected behavior**
What you expected to happen.

**Environment:**
- OS: [e.g. Windows 10]
- .NET Version: [e.g. 10.0]
- AppCore Version: [e.g. 2.0.1]

**Additional context**
Add any other context about the problem here.
```

## ✨ Feature Requests

### Feature Request Template

```markdown
**Is your feature request related to a problem?**
A clear description of what the problem is.

**Describe the solution you'd like**
A clear description of what you want to happen.

**Describe alternatives you've considered**
Alternative solutions or features you've considered.

**Additional context**
Add any other context about the feature request here.
```

## 📦 Release Process

### Semantic Versioning

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Checklist

- [ ] Version updated in project files
- [ ] CHANGELOG.md updated
- [ ] Migration guide updated (for major versions)
- [ ] All tests pass
- [ ] Documentation reviewed
- [ ] Package tested locally

## 💬 Communication

### Where to Ask Questions

- **General questions**: [GitHub Discussions](https://github.com/Harol-Reina/AppCore/discussions)
- **Bug reports**: [GitHub Issues](https://github.com/Harol-Reina/AppCore/issues)
- **Feature requests**: [GitHub Issues](https://github.com/Harol-Reina/AppCore/issues)

### Code of Conduct

Be respectful and inclusive. We want everyone to feel welcome to contribute.

---

## 🏆 Recognition

Contributors will be recognized in:

- README.md contributors section
- CHANGELOG.md for significant contributions
- GitHub releases for major contributions

Thank you for contributing to AppCore! 🎉