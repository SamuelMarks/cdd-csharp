using System;
using System.Collections.Generic;
using Cdd.OpenApi.Models;
using Cdd.OpenApi;
class Program {
    static void Main() {
        var schemas = new Dictionary<string, OpenApiSchema> {
            ["MyModel"] = new OpenApiSchema {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema> {
                    ["Name"] = new OpenApiSchema { Type = "string" },
                    ["Data"] = new OpenApiSchema { Type = "object" }
                }
            }
        };
        var ns = Cdd.OpenApi.Orm.Emit.ToDbContext("TestProject", schemas);
        Console.WriteLine(Cdd.OpenApi.WasmSafeFormatter.Format(ns));
    }
}
