# Usage of `cdd-csharp`

## Installation
The `cdd-csharp` package is available as a .NET global tool on NuGet:

```bash
dotnet tool install --global cdd-csharp
```

## CLI Usage

### Generate C# Code from OpenAPI 3.2.0 / Swagger 2.0
Generate an ASP.NET Core Minimal API & Entity Framework Core server implementation:
```bash
cdd-csharp from_openapi to_server -i spec.json -o ./src/MyApp.Api --tests
```

Generate a `C#` client SDK:
```bash
cdd-csharp from_openapi to_sdk -i spec.json -o ./src/MyApp.Client --tests
```

Generate a client SDK with a typed CLI and MCP adapter:
```bash
cdd-csharp from_openapi to_sdk_cli -i spec.json -o ./src/MyApp.Cli --tests --mcp
```

Batch-generate from a directory of specifications without scaffolding CI or `.csproj`/`.sln` files:
```bash
cdd-csharp from_openapi -d ./specs -o ./src/MyApp --no-github-actions --no-installable-package
```

### Extract OpenAPI 3.2.0 from C# Source Code
Statically parse C# controllers, Minimal API routes, models, clients, and XML docstrings into an OpenAPI 3.2.0 specification:
```bash
cdd-csharp to_openapi -i ./src/Controllers -o openapi.json
```

### Generate and Extract Arazzo 1.1.0 Workflows
Compile an Arazzo 1.1.0 workflow specification into C# workflow orchestrators, CLI runners, or server integration tests:
```bash
cdd-csharp from_arazzo to_sdk -i workflow.arazzo.json -o ./workflows --tests
cdd-csharp from_arazzo to_sdk_cli -i workflow.arazzo.json -o ./workflow-cli --tests
cdd-csharp from_arazzo to_server -i workflow.arazzo.json -o ./workflow-server --tests
```

Extract an Arazzo 1.1.0 specification from C# workflow source code:
```bash
cdd-csharp to_arazzo -i ./workflows -o workflow.arazzo.json
```

### Synchronize Code and Specifications In-Place (`sync`)
Synchronize using a designated source of truth (`class`, `function`, `openapi`, `arazzo`, or `workflow`):
```bash
cdd-csharp sync -t openapi -i spec.json -o ./src/MyApp
cdd-csharp sync -t class -i ./src/MyApp -o spec.json
cdd-csharp sync -t arazzo -i workflow.arazzo.json -o ./workflows
cdd-csharp sync -t workflow -i ./workflows -o workflow.arazzo.json
```

### Generate Documentation JSON for Docs Sites
Generate concise C# request snippets for an OpenAPI specification (see [TO_DOCS_JSON.md](TO_DOCS_JSON.md)):
```bash
cdd-csharp to_docs_json --no-imports --no-wrapping -i spec.json -o docs.json
```

### Start as an MCP or JSON-RPC 2.0 Server
Run as a Model Context Protocol (MCP) server over `stdio`:
```bash
cdd-csharp mcp
```

Expose all CLI functionality over a JSON-RPC 2.0 HTTP endpoint:
```bash
cdd-csharp serve_json_rpc -p 8080 -l 127.0.0.1
```

### Running Generated Servers
Servers generated via `from_openapi to_server` support ephemeral SQLite, PostgreSQL, and topological Bogus fake data seeding out of the box:
```bash
dotnet run --project ./src/MyApp.Api/src/GeneratedProject/GeneratedProject.csproj -- --ephemeral --seed
```
