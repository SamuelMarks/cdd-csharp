# Usage Guide

## CLI

```bash
# Display help
cdd-csharp --help

# Generate a C# SDK from OpenAPI
cdd-csharp from_openapi -i spec.json -o ./output_dir

# Parse C# code to OpenAPI
cdd-csharp to_openapi -f ./src -o openapi.json

# Extract code examples for documentation
cdd-csharp to_docs_json --no-imports --no-wrapping -i spec.json
```

## Programmatic API

Add a reference to `Cdd.OpenApi` in your .csproj.
You can use `OpenApiParser`, `OpenApiEmitter`, `CodeGenerator` and `SpecGenerator` classes.
