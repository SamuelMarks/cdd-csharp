# Usage Guide

The `cdd-openapi` tool can be used either as a command-line interface or as a C# class library.

## Command Line Interface (CLI)

Ensure the CLI is installed or run it directly via `dotnet run`.

### 1. Generating OpenAPI Spec from C# Code (`to_openapi`)

If you have a folder of C# models and controllers, you can generate an `openapi.json` from them:

```bash
cdd-openapi to_openapi -i ./src/MyBackend/Controllers/ -o api-spec.json
```

**What it does:**
- Finds all `.cs` files.
- Extracts POCO models into `components/schemas`.
- Extracts `[HttpGet("/path")]` style methods into `paths`.
- Extracts `/// <summary>` tags into `descriptions`.

### 2. Generating C# Code from OpenAPI Spec (`from_openapi`)

If you have a design-first `openapi.json`, you can scaffold your backend or create a client SDK:

```bash
cdd-openapi from_openapi -i api-spec.json -o ./src/MyClient/Generated/
```

**What it does:**
- Creates a `Models/` folder with strongly typed C# classes representing the API schemas.
- Creates an `IApi.cs` interface with abstract method definitions mapped to the HTTP routes and required parameters.

### 3. Formatting / Upgrading a Spec

To parse a spec and emit it back out cleanly formatted:

```bash
cdd-openapi emit old-spec.json new-spec.json
```

---

## Library Usage

Add the `Cdd.OpenApi` NuGet package to your project.

### Parsing JSON
```csharp
using Cdd.OpenApi.Parse;

var json = File.ReadAllText("spec.json");
var parser = new OpenApiParser();
var document = parser.ParseJson(json);

Console.WriteLine(document.Info.Title);
```

### Building a Spec in Code
```csharp
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Emit;

var doc = new OpenApiDocument
{
    OpenApi = "3.2.0",
    Info = new OpenApiInfo { Title = "My API", Version = "1.0.0" },
    Paths = new OpenApiPaths
    {
        ["/health"] = new OpenApiPathItem 
        {
            Get = new OpenApiOperation { OperationId = "CheckHealth" }
        }
    }
};

var emitter = new OpenApiEmitter();
File.WriteAllText("health-spec.json", emitter.EmitJson(doc));
```

### Code Generation via API
```csharp
using Cdd.OpenApi;

// From C# string to Spec
var spec = SpecGenerator.Generate(new[] { "public class User { public int Id { get; set; } }" });

// From Spec to C# string
var codeFiles = CodeGenerator.Generate(spec, "My.Custom.Namespace");
foreach(var file in codeFiles) {
    Console.WriteLine(file.FileName);
    Console.WriteLine(file.Code);
}
```