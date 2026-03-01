# Usage of `cdd-csharp`

## Installation
The `cdd-csharp` package is available on NuGet.

```bash
dotnet tool install --global cdd-csharp
```

## CLI Usage

### Generate code from OpenAPI
Generate a server implementation:
```bash
cdd-csharp from_openapi to_server -i spec.json -o ./src/MyApp.Api
```

Generate a client SDK:
```bash
cdd-csharp from_openapi to_sdk -i spec.json -o ./src/MyApp.Client
```

Generate an SDK with a fully typed CLI:
```bash
cdd-csharp from_openapi to_sdk_cli -i spec.json -o ./src/MyApp.Cli
```

### Parse C# code to OpenAPI
Parse controllers and models to a single OpenAPI file:
```bash
cdd-csharp to_openapi -f ./src/Controllers -o spec.json
```

### Generate Documentation JSON for Docs Site
Generate concise how-to-call snippets:
```bash
cdd-csharp to_docs_json --no-imports --no-wrapping -i spec.json -o docs.json
```

### Start as a JSON-RPC Server
Expose all functionality over a JSON-RPC API:
```bash
cdd-csharp serve_json_rpc --port 8080 --listen 127.0.0.1
```
