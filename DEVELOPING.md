# Developing cdd-csharp

## Prerequisites
- .NET SDK (9.0+)
- Make

## Building the project
Run `make build` to restore packages and build all projects including the CLI.

## Running Tests
Run `make test` to execute all tests under `tests/`.

## Contributing Code
1. Use Roslyn APIs to parse C#. Please refrain from using Reflection for code analysis.
2. Adhere to modular architecture: keep Parsers (`Parse.cs`) and Emitters (`Emit.cs`) distinct in their respective modules (e.g. `src/Cdd.OpenApi/Classes/`).
3. Maintain 100% test coverage and docstring coverage for any newly introduced code. Run the `pre-commit` script to update badges.

## Pre-commit Hooks
This project uses `.pre-commit-config.yaml`. To install the hooks:
```bash
pip install pre-commit
pre-commit install
```
