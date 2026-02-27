# Architecture

The CDD C# OpenAPI tool is designed around a modular, bidirectional pipeline connecting **C# Syntax Trees** (via Roslyn) and **OpenAPI JSON Specifications**. 

The architecture is split into three primary layers: **Models**, **Parsing/Emission**, and **Orchestration**.

## 1. Models Layer (`src/Cdd.OpenApi/Models`)

This layer consists of pure C# POCOs (Plain Old C# Objects) that strictly map to the [OpenAPI 3.2.0 Specification](https://spec.openapis.org/oas/v3.1.0). 
- `OpenApiDocument` acts as the root node.
- Extensive use of `System.Text.Json.Serialization` attributes ensure that properties serialize cleanly (e.g., `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`).
- These models hold no business logic; they serve purely as the strongly-typed intermediate representation (IR) in memory.

## 2. Parsing and Emission Layers

This layer acts as the translation bridge. There are two distinct pipelines: **JSON I/O** and **Roslyn AST I/O**.

### JSON I/O (`src/Cdd.OpenApi/Parse` & `Emit`)
- `OpenApiParser`: Deserializes a raw JSON string into the `OpenApiDocument` IR using `System.Text.Json`.
- `OpenApiEmitter`: Serializes the `OpenApiDocument` IR back into formatted JSON.

### Roslyn AST I/O
Instead of dealing with raw strings of code, the tool deeply understands C# via the `.NET Compiler Platform (Roslyn)`. It consists of modular domain-specific parsers and emitters:

- **Classes (`src/Cdd.OpenApi/Classes`)**:
  - `Parse.ToSchema`: Converts a C# `ClassDeclarationSyntax` into an `OpenApiSchema` (`type: object`), mapping primitive C# types (int, bool, string) to OpenAPI equivalents.
  - `Emit.ToClass`: Converts an `OpenApiSchema` back into a C# `ClassDeclarationSyntax`, handling nullable annotations (`?`) based on the schema's `required` arrays.

- **Routes (`src/Cdd.OpenApi/Routes`)**:
  - `Parse.ToPaths`: Examines `MethodDeclarationSyntax` within a controller. It identifies attributes like `[HttpGet("/pets/{id}")]` and transforms them into `OpenApiPaths`, mapping method parameters to route/query parameters.
  - `Emit.ToInterface`: Converts `OpenApiPaths` into a C# `InterfaceDeclarationSyntax`, outputting an abstract contract of the API.

- **Docstrings (`src/Cdd.OpenApi/Docstrings`)**:
  - Extracts `/// <summary>` XML trivia from Roslyn nodes and maps them to the OpenAPI `description` and `summary` fields, and vice versa.

## 3. Orchestration Layer (`src/Cdd.OpenApi/SpecGenerator.cs` & `CodeGenerator.cs`)

This layer coordinates the parsers and emitters to perform the high-level commands executed by the CLI.
- `SpecGenerator`: Ingests C# source text, builds the Roslyn `CSharpSyntaxTree`, isolates Models and Controllers, runs them through the AST Parsers, and aggregates them into a final `OpenApiDocument`.
- `CodeGenerator`: Takes an `OpenApiDocument`, loops through its schemas and paths, runs the AST Emitters, and produces `GeneratedCode` objects containing the raw C# text to be written to disk.

## Summary Flow
**Code First:** `C# Code -> Roslyn SyntaxTree -> AST Parsers -> OpenApiDocument -> JSON Emitter -> spec.json`
**Spec First:** `spec.json -> JSON Parser -> OpenApiDocument -> AST Emitters -> Roslyn SyntaxTree -> C# Code`