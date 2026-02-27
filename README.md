# CDD C# OpenAPI Tools

[![NuGet](https://img.shields.io/nuget/v/Cdd.OpenApi.svg)](https://www.nuget.org/packages/Cdd.OpenApi/)
[![Test Coverage](https://img.shields.io/badge/Coverage-100%25-brightgreen.svg)]()
[![Doc Coverage](https://img.shields.io/badge/Docs-10%25-red.svg)]()

This repository contains a C# library and CLI for parsing and emitting OpenAPI 3.2.0 definitions, mapping them bidirectionally to strongly typed C# syntax using the Roslyn compiler platform.

## Why CDD OpenAPI?
"Contract-Driven Development" ensures your API specs and code never drift apart. This tool enables two workflows seamlessly:
1.  **Code-First**: Write standard C# classes and ASP.NET Core controllers, and generate a flawless OpenAPI JSON specification automatically.
2.  **API-First**: Author an `openapi.json` spec, and generate abstract C# Interfaces and POCO Models to build your backend or client SDK around.

## Standard Commands (CLI)

The CLI implements the standard bidirectional interface:

Generate C# code (Interfaces & Models) from an OpenAPI JSON document:
```bash
cdd-openapi from_openapi -i spec.json -o ./src/MyClient
```

Generate an OpenAPI JSON document from C# source files/directories:
```bash
cdd-openapi to_openapi -i ./src/Controllers -o generated-spec.json
```

See [USAGE.md](USAGE.md) for more details.

## Features Supported (OpenAPI 3.2.0)

- Basic structure (Info, Paths, Components, Servers)
- Schemas (from C# Classes and Properties)
- Content/Media Types
- Operations (GET, POST, PUT, DELETE, etc., mapped to `[HttpGet]` etc.)
- Parameters (Path, Query)
- XML Docstrings mapping to OpenApi Descriptions and Summaries
- Nullability mapping to OpenAPI `required` arrays.

## Project Structure

- `src/Cdd.OpenApi`: Class library containing the models, Roslyn-powered code parsers/emitters, and JSON handlers.
- `src/Cdd.OpenApi.Cli`: Command-line interface for the library.
- `tests/Cdd.OpenApi.Tests`: Unit and integration tests.

For architectural details see [ARCHITECTURE.md](ARCHITECTURE.md).

For development details see [DEVELOPING.md](DEVELOPING.md).

For compliance to the spec see [COMPLIANCE.md](COMPLIANCE.md).