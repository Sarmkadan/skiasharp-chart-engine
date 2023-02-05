# Contributing to SkiaSharp Chart Engine

We welcome contributions! Please follow these guidelines:

1. **Fork & Branch**: Fork the repo, then create a feature branch (`git checkout -b feature/name`).
2. **Code**: Follow existing C# style (indent 4, C# conventions) and add XML docs for public APIs.
3. **Test**: Run `make test` before submitting. Add new tests for all features/fixes.
4. **Style**: Keep methods under 50 lines.
5. **PR**: Open a PR referencing the relevant issue, with a concise description.

### Development Setup
```bash
git clone https://github.com/vladyslav-zaiets/skiasharp-chart-engine.git
cd skiasharp-chart-engine
dotnet build
dotnet test
```

### Reporting Issues
Please include a minimal reproduction, expected/actual behavior, and environment details in bug reports.

License: MIT.
