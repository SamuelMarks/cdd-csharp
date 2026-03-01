# Developing cdd-csharp

## Prerequisites
- .NET 10.0 SDK
- GNU Make or a Windows command prompt (for `make.bat`)

## Getting Started
```bash
make install_deps
make build
make test
```

## Structure
- `src/Cdd.OpenApi/`: The core CDD library (parsers, emitters, AST manipulation).
- `src/Cdd.OpenApi.Cli/`: The command-line interface executable.
- `tests/Cdd.OpenApi.Tests/`: Unit and integration tests with xUnit.
