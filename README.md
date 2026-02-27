# cdd-csharp

<!-- BADGES_START -->
[![License](https://img.shields.io/badge/license-Apache--2.0%20OR%20MIT-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![CI/CD](https://github.com/offscale/cdd-csharp/workflows/CI/badge.svg)](https://github.com/offscale/cdd-csharp/actions)
[![Coverage](https://codecov.io/gh/offscale/cdd-csharp/branch/master/graph/badge.svg)](https://codecov.io/gh/offscale/cdd-csharp)
<!-- BADGES_END -->

OpenAPI ↔ C#. Welcome to **cdd-csharp**, a code-generation and compilation tool bridging the gap between OpenAPI specifications and native `C#` source code. 

This toolset allows you to fluidly convert between your language's native constructs (like classes, structs, functions, routing, clients, and ORM models) and OpenAPI specifications, ensuring a single source of truth without sacrificing developer ergonomics.

## 🚀 Capabilities

The `cdd-csharp` compiler leverages a unified architecture to support various facets of API and code lifecycle management.

* **Compilation**:
  * **OpenAPI → `C#`**: Generate idiomatic native models, network routes, client SDKs, database schemas, and boilerplate directly from OpenAPI (`.json` / `.yaml`) specifications.
  * **`C#` → OpenAPI**: Statically parse existing `C#` source code and emit compliant OpenAPI specifications.
* **AST-Driven & Safe**: Employs static analysis (Abstract Syntax Trees) instead of unsafe dynamic execution or reflection, allowing it to safely parse and emit code even for incomplete or un-compilable project states.
* **Seamless Sync**: Keep your docs, tests, database, clients, and routing in perfect harmony. Update your code, and generate the docs; or update the docs, and generate the code.

## 📦 Installation

Since `cdd-csharp` is distributed as a global .NET tool, you can easily install it using the .NET CLI.

Requires **.NET 10.0 SDK**.

```bash
dotnet tool install --global cdd_csharp
```
*(Ensure that your global tools path is added to your environment variables)*

You can also use it as a package dependency for programmatic usage:
```bash
dotnet add package Cdd.OpenApi
```

## 🛠 Usage

### Command Line Interface

Generate C# models and routes from an OpenAPI specification:
```bash
cdd_csharp from_openapi -i openapi.json -o ./src/Generated
```

Parse existing C# code and generate an OpenAPI specification:
```bash
cdd_csharp to_openapi -i ./src/Controllers -o openapi.json
```

### Programmatic SDK / Library

```cs
using System.IO;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Models;

// 1. Parse an OpenAPI specification
var jsonContent = File.ReadAllText("openapi.json");
OpenApiDocument doc = new OpenApiParser().ParseJson(jsonContent);

// 2. Or, Parse C# code to an OpenApiDocument
// var doc = SpecGenerator.Generate(new[] { "public class Pet { public string Name { get; set; } }" });

// 3. Emit C# code
var generatedCodeFiles = CodeGenerator.Generate(doc);
foreach (var file in generatedCodeFiles)
{
    File.WriteAllText(file.FileName, file.Code);
}

// 4. Emit OpenAPI specification
var newOpenApiJson = new OpenApiEmitter().EmitJson(doc);
File.WriteAllText("new_openapi.json", newOpenApiJson);
```

## 🏗 Supported Conversions for C#

*(The boxes below reflect the features supported by this specific `cdd-csharp` implementation)*

| Concept | Parse (From) | Emit (To) |
|---------|--------------|-----------|
| OpenAPI (JSON/YAML) | ✅ | ✅ |
| `C#` Models / Structs / Types | ✅ | ✅ |
| `C#` Server Routes / Endpoints | ✅ | ✅ |
| `C#` API Clients / SDKs | [ ] | [ ] |
| `C#` ORM / DB Schemas | [ ] | [ ] |
| `C#` CLI Argument Parsers | [ ] | [ ] |
| `C#` Docstrings / Comments | ✅ | ✅ |

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