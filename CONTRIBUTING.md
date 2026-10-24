# Contributing to SkiaSharp Chart Engine

Thanks for helping out. This guide covers the setup, checks, and PR process for this repo.

## Prerequisites

- **.NET SDK 10** (`global.json` pins `10.0.100` with `rollForward: latestMinor`; the library targets `net10.0`). Check with `dotnet --version`.
- **git**
- Optional: **Docker** for `make docker-build` / `make docker-run` (port 5000).

## Build

The root folder contains both a `.sln`/`.slnx` and a `.csproj`, so a bare `dotnet build` fails with MSB1011. Name the project explicitly:

```bash
dotnet restore skiasharp-chart-engine.csproj
dotnet build skiasharp-chart-engine.csproj -c Release   # or: make build
```

## Test

```bash
dotnet test tests/skiasharp-chart-engine.Tests/skiasharp-chart-engine.Tests.csproj -c Release
```

Tests use xUnit, FluentAssertions 7, and Moq. Prefer the explicit path above: `make test` points at the main `.csproj`.

## Coding conventions

- **Formatting:** `.editorconfig` is authoritative. 4 spaces, Allman braces (opening brace on its own line), `var` only when the type is apparent.
- **Nullable reference types and implicit usings are enabled** (`Directory.Build.props`). Don't add `#nullable disable`.
- **Naming:** PascalCase for types and members, `I` prefix for interfaces (one per file, next to its implementation), `_camelCase` for private fields, `Async` suffix and a `CancellationToken` parameter on async methods.
- **Namespaces:** file-scoped, matching the folder (`SkiaSharpChartEngine.<Folder>`). One public type per file.
- **Dependencies:** resolve services through DI (`Microsoft.Extensions.DependencyInjection`); log with `ILogger<T>`. Null-check constructor arguments with `?? throw new ArgumentNullException(nameof(x))`.
- **Docs:** XML doc comments on all public APIs.
- **Size:** keep methods under ~50 lines.
- **Build exclusions:** `skiasharp-chart-engine.csproj` has a large `<Compile Remove>` list (e.g. `API/`, `CLI/`, `Middleware/`, `Workers/`, `Rendering/`). Check it before assuming a new file compiles.
- Match the file header banner used by existing `.cs` files.

## Tests

- Add tests for every new feature and bug fix.
- Place them in `tests/skiasharp-chart-engine.Tests/` as `<Class>Tests.cs`, optionally split into `<Class>TestsExtensions.cs` or `<Class>TestsValidation.cs`.

## Commit messages

Use conventional prefixes, as in the existing history: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`.

```
fix: reject negative bar widths in bar chart rendering
```

- Imperative summary line, short and specific.
- Add a body when the "why" isn't obvious from the diff.
- One logical change per commit.

## Pull requests

1. **Branch from up-to-date `main`.** External contributors should fork first. Name branches `feature/<name>` or `fix/<name>`.
2. **Keep PRs focused.** One concern per PR; put unrelated refactors in separate PRs.
3. **Make sure checks pass locally.** The build and test commands above must succeed. CI (`.github/workflows/ci.yml`) runs the same restore, build, and test steps on PRs to `main`.
4. **Fill in the PR template** (`.github/PULL_REQUEST_TEMPLATE.md`): summary, changes, type of change, testing, and checklist.
5. **Update docs** (`docs/` and `README.md`) when public API behavior changes.

Bug reports go through `.github/ISSUE_TEMPLATE/bug_report.md`. Include a minimal reproduction, expected and actual behavior, and environment details. For security issues, follow [SECURITY.md](SECURITY.md) instead of opening a public issue.

By contributing, you agree that your changes are licensed under the [MIT License](LICENSE).
