using System.Collections.Generic;
using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Orm;

namespace Cdd.OpenApi.Tests.Orm
{
    public class SeederGeneratorTests
    {
        [Fact]
        public void GenerateSeeder_Coverage()
        {
            var schemas = new Dictionary<string, OpenApiSchema>
            {
                ["TestModel"] = new OpenApiSchema
                {
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["EmailAddress"] = new OpenApiSchema { Type = "string" },
                        ["FullName"] = new OpenApiSchema { Type = "string" },
                        ["Id"] = new OpenApiSchema { Type = "string" },
                        ["Description"] = new OpenApiSchema { Type = "string" },
                        ["Age"] = new OpenApiSchema { Type = "integer" },
                        ["IntId"] = new OpenApiSchema { Type = "integer" },
                        ["IsActive"] = new OpenApiSchema { Type = "boolean" }
                    }
                }
            };

            var code = SeederGenerator.GenerateSeeder(schemas, "GeneratedProject", true);

            Assert.Contains("f.Internet.Email()", code[1].Code);
            Assert.Contains("f.Name.FullName()", code[1].Code);
            Assert.Contains("Guid.NewGuid().ToString()", code[1].Code);
            Assert.Contains("f.Lorem.Word()", code[1].Code);
            Assert.Contains("f.Random.Int(1, 100)", code[1].Code);
            Assert.Contains("f.Random.Bool()", code[1].Code);

            // Reassign IntId to id so it skips
            schemas["TestModel"].Properties.Remove("IntId");
            schemas["TestModel"].Properties["id"] = new OpenApiSchema { Type = "integer" };
            SeederGenerator.GenerateSeeder(schemas, "GeneratedProject", true);
        }
    }
}
