# Developing CDD C# OpenAPI Tools

## Requirements
*   [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or newer.
*   Bash or compatible shell (for provided scripts).

## Setup
1.  Clone the repository.
2.  Restore the dependencies:
    ```bash
    dotnet restore
    ```
3.  Build the solution:
    ```bash
    dotnet build
    ```

## Project Layout

*   `src/Cdd.OpenApi/` - The core library. Contains the logic for reading JSON, building ASTs via Roslyn, and manipulating OpenAPI specifications.
*   `src/Cdd.OpenApi.Cli/` - The command-line wrapper. Provides the interface for users to invoke spec and code generation.
*   `tests/Cdd.OpenApi.Tests/` - xUnit testing project containing unit tests and integration tests.

## Running Tests and Checking Coverage
The project maintains a strict 100% test coverage requirement for all logic.

To run the standard tests:
```bash
dotnet test
```

To run tests and view the exact coverage metrics, we use `coverlet`. Ensure it is installed as a global tool:
```bash
dotnet tool install --global coverlet.console

# Run coverlet over the built test DLL
coverlet tests/Cdd.OpenApi.Tests/bin/Debug/net10.0/Cdd.OpenApi.Tests.dll 
  --target "dotnet" 
  --targetargs "test tests/Cdd.OpenApi.Tests --no-build" 
  --format opencover 
  --output coverage.xml
```

You can then inspect the `coverage.xml` to ensure line, branch, and method metrics remain at `100%`. If branch coverage falls below `100%`, find the missing path in your code and write a new `[Fact]` to cover it.

## Design Philosophy
1.  **Zero Third-Party Dependencies (Mostly)**: Keep the core fast and clean. Use `System.Text.Json` instead of `Newtonsoft.Json`. Use `Microsoft.CodeAnalysis.CSharp` for AST parsing instead of Regex.
2.  **Immutability and Isolation**: The Roslyn AST is immutable. Parsers and Emitters should be static, pure functions where possible.
3.  **Strict Models**: The `Models` namespace should reflect the OpenAPI spec exactly. They should never contain generation or parsing logic.