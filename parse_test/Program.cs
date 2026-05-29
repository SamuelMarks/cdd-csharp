using System;
using System.Text.Json;
using Cdd.OpenApi.Parse;

class Program {
    static void Main() {
        var json = "{\"openapi\": \"3.0.0\", \"paths\": {\"/pet\": {}}}";
        var doc = JsonSerializer.Deserialize(json, typeof(Cdd.OpenApi.Models.OpenApiDocument), new JsonSerializerOptions { TypeInfoResolver = OpenApiJsonContext.Default }) as Cdd.OpenApi.Models.OpenApiDocument;
        Console.WriteLine(doc != null && doc.Paths != null ? "PATHS: " + doc.Paths.Count : "PATHS: 0");
    }
}
