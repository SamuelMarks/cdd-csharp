cdd-csharp
==========

[![License](https://img.shields.io/badge/license-Apache--2.0%20OR%20MIT-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![CI/CD](https://github.com/offscale/cdd-csharp/workflows/CI/badge.svg)](https://github.com/offscale/cdd-csharp/actions)
[![Test Coverage](https://img.shields.io/badge/Test%20Coverage-100%25-brightgreen.svg)]()
[![Doc Coverage](https://img.shields.io/badge/Doc%20Coverage-100%25-brightgreen.svg)]()

OpenAPI ↔ C#. This is one compiler in a suite, all focussed on the same task: Compiler Driven Development (CDD).

Each compiler is written in its target language, is whitespace and comment sensitive, and has both an SDK and CLI.

The CLI—at a minimum—has:
- `cdd-csharp --help`
- `cdd-csharp --version`
- `cdd-csharp from_openapi -i spec.json`
- `cdd-csharp to_openapi -f path/to/code`
- `cdd-csharp to_docs_json --no-imports --no-wrapping -i spec.json`

The goal of this project is to enable rapid application development without tradeoffs. Tradeoffs of Protocol Buffers / Thrift etc. are an untouchable "generated" directory and package, compile-time and/or runtime overhead. Tradeoffs of Java or JavaScript for everything are: overhead in hardware access, offline mode, ML inefficiency, and more. And neither of these alterantive approaches are truly integrated into your target system, test frameworks, and bigger abstractions you build in your app. Tradeoffs in CDD are code duplication (but CDD handles the synchronisation for you).

## 🚀 Capabilities

The `cdd-csharp` compiler leverages a unified architecture to support various facets of API and code lifecycle management.

* **Compilation**:
  * **OpenAPI → `C#`**: Generate idiomatic native models, network routes, client SDKs, database schemas, and boilerplate directly from OpenAPI (`.json` / `.yaml`) specifications.
  * **`C#` → OpenAPI**: Statically parse existing `C#` source code and emit compliant OpenAPI specifications.
* **AST-Driven & Safe**: Employs static analysis (Abstract Syntax Trees) instead of unsafe dynamic execution or reflection, allowing it to safely parse and emit code even for incomplete or un-compilable project states.
* **Seamless Sync**: Keep your docs, tests, database, clients, and routing in perfect harmony. Update your code, and generate the docs; or update the docs, and generate the code.

## 📦 Installation

Requires .NET 10.0 runtime.

Install as a global tool:
```bash
dotnet tool install --global cdd-csharp --version 0.0.1
```
Or clone the repository and build:
```bash
make build
```

## 🛠 Usage

### Command Line Interface

```bash
# Generate C# models and routes from an OpenAPI spec
cdd-csharp from_openapi to_server -i openapi.json -o ./src/Generated

# Generate a C# API client SDK CLI from a spec
cdd-csharp from_openapi to_sdk_cli -i openapi.json -o ./src/GeneratedCli

# Parse existing C# code to emit a new OpenAPI spec
cdd-csharp to_openapi -f ./src/Controllers -o openapi.yaml
```

### Programmatic SDK / Library

```csharp
using System.IO;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi;

// Parse a spec
var parser = new OpenApiParser();
var doc = parser.ParseJson(File.ReadAllText("openapi.json"));

// Generate C# Code
var generatedCodes = CodeGenerator.Generate(doc, "Generated", GenerateType.All);

// Re-parse C# code
var codes = generatedCodes.Select(c => c.Code).ToList();
var reParsedDoc = SpecGenerator.Generate(codes);

// Emit OpenAPI spec
var emitter = new OpenApiEmitter();
File.WriteAllText("re-parsed.json", emitter.EmitJson(reParsedDoc));
```

## Design choices

`cdd-csharp` uses the powerful Roslyn compiler (`Microsoft.CodeAnalysis`) for parsing and generating C# code without relying on potentially slow and unsafe reflection. It performs syntactic parsing and analysis natively. C# features like AST mutation support make lossless transformation robust and seamless. 

## 🏗 Supported Conversions for C#

*(The boxes below reflect the features supported by this specific `cdd-csharp` implementation)*

| Concept | Parse (From) | Emit (To) |
|---------|--------------|-----------|
| OpenAPI (JSON/YAML) | [✅] | [✅] |
| `C#` Models / Structs / Types | [✅] | [✅] |
| `C#` Server Routes / Endpoints | [✅] | [✅] |
| `C#` API Clients / SDKs | [✅] | [✅] |
| `C#` ORM / DB Schemas | [ ] | [✅] |
| `C#` CLI Argument Parsers | [✅] | [✅] |
| `C#` Docstrings / Comments | [✅] | [✅] |

---

## License

Licensed under either of

- Apache License, Version 2.0 ([LICENSE-APACHE](LICENSE-APACHE) or <https://www.apache.org/licenses/LICENSE-2.0>)
- MIT license ([LICENSE-MIT](LICENSE-MIT) or <https://opensource.org/licenses/MIT>)

at your option.

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted
for inclusion in the work by you, as defined in the Apache-2.0 license, shall be
dual licensed as above, without any additional terms or conditions.