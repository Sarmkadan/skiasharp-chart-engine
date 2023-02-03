# Contributing to SkiaSharp Chart Engine

Thank you for your interest in contributing to the SkiaSharp Chart Engine! This document provides guidelines and instructions for contributing to our open-source project.

## Code of Conduct

We are committed to providing a welcoming and inspiring community for all. Please read and adhere to our [Code of Conduct](CODE_OF_CONDUCT.md) before participating in the project.

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022, VS Code, or Rider
- Git

### Development Setup

1. **Fork the Repository**
   ```bash
   # Visit https://github.com/sarmkadan/skiasharp-chart-engine
   # Click "Fork" to create your copy
   ```

2. **Clone Your Fork**
   ```bash
   git clone https://github.com/YOUR_USERNAME/skiasharp-chart-engine.git
   cd skiasharp-chart-engine
   ```

3. **Add Upstream Remote**
   ```bash
   git remote add upstream https://github.com/sarmkadan/skiasharp-chart-engine.git
   ```

4. **Create Development Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

5. **Build and Test**
   ```bash
   make build
   make test
   ```

6. **Run the Sample Project**

   The `examples/v2-basic-usage/` project is a runnable console app demonstrating the core API. Start here to verify your environment before making changes:

   ```bash
   cd examples/v2-basic-usage
   dotnet run
   ```

   Rendered PNG files are written to the current directory. To run a specific example from the `examples/` folder, reference it directly:

   ```bash
   dotnet script examples/BasicLineChart.cs   # requires dotnet-script global tool
   # — or —
   dotnet run --project examples/v2-basic-usage
   ```

   **Verifying your change end-to-end**

   After editing library code, rebuild and re-run the sample to confirm the output looks correct:

   ```bash
   dotnet build skiasharp-chart-engine.sln
   dotnet run --project examples/v2-basic-usage
   ```

## Development Workflow

### Making Changes

1. **Keep Changes Focused**
   - One feature or fix per pull request
   - Keep commits atomic and logical
   - Write descriptive commit messages

2. **Code Style**
   - Follow C# naming conventions
   - Use the `.editorconfig` file for consistency
   - Keep methods under 50 lines
   - Add XML documentation to public APIs

3. **Testing**
   - Write tests for new features
   - Ensure all tests pass: `make test`
   - Aim for >80% code coverage

4. **Documentation**
   - Update README if behavior changes
   - Add XML comments to public methods
   - Update CHANGELOG.md
   - Update relevant docs in `/docs`

### Commit Guidelines

Write clear, descriptive commit messages:

```
feature: Add support for custom chart renderers

- Implement IChartRenderer interface
- Add plugin architecture
- Update documentation
- Add unit tests

Closes #123
```

**Format**: `type: description`

**Types**:
- `feature`: New functionality
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Test additions/changes
- `perf`: Performance improvements
- `ci`: CI/CD changes
- `chore`: Dependency updates, etc.

### Pull Request Process

1. **Update from Upstream**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Run Local CI**
   ```bash
   dotnet restore && dotnet build --configuration Release && dotnet test --no-build --configuration Release --verbosity normal
   ```

3. **Push Changes**
   ```bash
   git push origin feature/your-feature-name
   ```

4. **Create Pull Request**
   - Go to GitHub and create PR
   - Reference related issues: `Fixes #123`
   - Fill out the PR template
   - Wait for review

5. **Respond to Feedback**
   - Address reviewer comments
   - Request re-review when ready
   - Keep discussion respectful

6. **Merge**
   - Maintainers will merge when approved
   - Your feature will be in next release!

## Coding Standards

### C# Code Style

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using SomeNamespace;

namespace SkiaSharpChartEngine.Feature;

/// <summary>
/// Brief description of class purpose
/// </summary>
public class FeatureClass
{
    private readonly IDependency _dependency;
    private string _internalField;

    public string PublicProperty { get; set; }

    public FeatureClass(IDependency dependency)
    {
        _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
    }

    /// <summary>
    /// Brief description of method purpose and behavior
    /// </summary>
    /// <param name="parameter">Parameter description</param>
    /// <returns>Return value description</returns>
    public string PublicMethod(string parameter)
    {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter));

        return _dependency.DoSomething(parameter);
    }

    private void PrivateMethod()
    {
        // Implementation
    }
}
```

**Guidelines**:
- Include file header on all .cs files
- Add XML documentation to public members
- Use meaningful names (avoid `x`, `y`, `temp`)
- Keep methods focused and concise
- Use async/await for I/O operations
- Use dependency injection for dependencies

### Testing

```csharp
[TestFixture]
public class FeatureTests
{
    private FeatureClass _feature;
    private Mock<IDependency> _dependencyMock;

    [SetUp]
    public void Setup()
    {
        _dependencyMock = new Mock<IDependency>();
        _feature = new FeatureClass(_dependencyMock.Object);
    }

    [Test]
    public void PublicMethod_WithValidInput_ReturnsExpectedValue()
    {
        // Arrange
        var input = "test";
        _dependencyMock
            .Setup(d => d.DoSomething(input))
            .Returns("expected");

        // Act
        var result = _feature.PublicMethod(input);

        // Assert
        Assert.That(result, Is.EqualTo("expected"));
    }

    [Test]
    public void PublicMethod_WithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _feature.PublicMethod(null));
    }
}
```

**Testing Guidelines**:
- Use AAA pattern (Arrange, Act, Assert)
- Test both success and failure cases
- Mock external dependencies
- Keep tests focused and readable
- Use descriptive test names

## Areas for Contribution

### High Priority

- [ ] Performance optimizations
- [ ] Bug fixes with reproducible cases
- [ ] Documentation improvements
- [ ] Example applications
- [ ] Test coverage improvements

### Medium Priority

- [ ] New chart types
- [ ] Export format support
- [ ] Template library expansion
- [ ] Localization/i18n
- [ ] Platform support (MAUI, etc.)

### Nice to Have

- [ ] Animation support
- [ ] GPU acceleration
- [ ] Custom themes
- [ ] Plugin system
- [ ] Web components

## Reporting Issues

### Bug Reports

Include:
- .NET version
- SkiaSharp version
- Minimal reproduction code
- Expected vs actual behavior
- Screenshots if UI-related

**Template**:
```markdown
## Description
Brief description of bug

## Reproduction Steps
1. Step one
2. Step two
3. Step three

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Environment
- OS: Windows 11
- .NET: 10.0.0
- SkiaSharpChartEngine: 1.2.0

## Code Example
(Minimal code to reproduce)
```

### Feature Requests

Include:
- Clear description of desired feature
- Use case and motivation
- Example code if applicable
- Alternative approaches considered

**Template**:
```markdown
## Description
Brief description of feature

## Motivation
Why this feature is needed

## Proposal
How it should work

## Example Usage
```csharp
// Code example
```

## Documentation Contributions

We welcome documentation improvements:

1. **README.md** - Installation, quick start
2. **docs/getting-started.md** - Step-by-step guide
3. **docs/architecture.md** - Design and internals
4. **docs/api-reference.md** - API documentation
5. **docs/deployment.md** - Production deployment
6. **docs/faq.md** - Common questions
7. **examples/** - Example programs

## Release Process

1. Create release branch: `release/vX.Y.Z`
2. Update CHANGELOG.md with changes
3. Update version in .csproj file
4. Create PR for review
5. Merge to main
6. Tag commit with version: `git tag vX.Y.Z`
7. Push tag: `git push origin vX.Y.Z`
8. GitHub Actions publishes to NuGet

## Getting Help

- 💬 **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/skiasharp-chart-engine/discussions)
- 🐛 **Issues**: [GitHub Issues](https://github.com/sarmkadan/skiasharp-chart-engine/issues)
- 📧 **Direct Contact**: Open an issue with question tag

## Recognition

Contributors will be recognized in:
- CHANGELOG.md
- GitHub contributor list
- Project documentation

## Legal

By contributing, you agree that your contributions will be licensed under the MIT License.

## Questions?

Don't hesitate to ask! Open a discussion or issue if you have questions about:
- How to contribute
- Where to contribute
- Code style questions
- Design decisions

---

Thank you for helping make SkiaSharp Chart Engine better! 🎉

Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect
