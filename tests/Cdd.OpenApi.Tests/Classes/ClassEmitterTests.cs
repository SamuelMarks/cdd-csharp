using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Classes;
using System.Collections.Generic;

namespace Cdd.OpenApi.Tests.Classes
{
    public class ClassEmitterTests
    {
        [Fact]
        public void ToClass_ValidSchema_GeneratesCorrectClass()
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Description = "A pet schema",
                Required = new List<string> { "Id" },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["Id"] = new OpenApiSchema { Type = "integer", Description = "Pet ID" },
                    ["Name"] = new OpenApiSchema { Type = "string" },
                    ["IsVaccinated"] = new OpenApiSchema { Type = "boolean" },
                    ["Data"] = new OpenApiSchema { Type = "unknown_type" }
                }
            };

            var classNode = Cdd.OpenApi.Classes.Emit.ToClass("Pet", schema);
            var code = classNode.ToFullString();

            Assert.Contains("/// <summary>", code);
            Assert.Contains("/// A pet schema", code);
            Assert.Contains("public class Pet", code);
            
            // Id is required
            Assert.Contains("public int Id { get; set; }", code);
            Assert.Contains("/// <summary>", code);
            Assert.Contains("/// Pet ID", code);

            // Name is optional string
            Assert.Contains("public string? Name { get; set; }", code);

            // IsVaccinated is optional bool
            Assert.Contains("public bool? IsVaccinated { get; set; }", code);
            
            // Data is unknown fallback to object
            Assert.Contains("public object Data { get; set; }", code);
        }

        [Fact]
        public void MapTypeToCSharp_MapsNumberToDouble()
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["Weight"] = new OpenApiSchema { Type = "number" }
                }
            };

            var classNode = Cdd.OpenApi.Classes.Emit.ToClass("Pet", schema);
            var code = classNode.ToFullString();

            Assert.Contains("public double? Weight { get; set; }", code);
        }
    }
}
