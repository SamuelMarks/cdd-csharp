using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Orm;

namespace Cdd.OpenApi.Tests.Orm
{
    public class OrmEmitTests
    {
        [Fact]
        public void ToDbContext_GeneratesModelBuilderIgnoreAndHasNoKey()
        {
            var schemas = new Dictionary<string, OpenApiSchema>
            {
                ["MyModel"] = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["Name"] = new OpenApiSchema { Type = "string" },
                        ["Data"] = new OpenApiSchema { Type = "object" },
                        ["List"] = new OpenApiSchema { Type = "array" },
                        ["Untyped"] = new OpenApiSchema()
                    }
                },
                ["NonObject"] = new OpenApiSchema { Type = "string" }
            };

            var ns = global::Cdd.OpenApi.Orm.Emit.ToDbContext("GeneratedProject", schemas);
            var code = ns.ToFormattedString();

            Assert.Contains("modelBuilder.Entity<MyModel>().Ignore(e=>e.Data);", code);
            Assert.Contains("modelBuilder.Entity<MyModel>().Ignore(e=>e.List);", code);
            Assert.Contains("modelBuilder.Entity<MyModel>().Ignore(e=>e.Untyped);", code);
            Assert.Contains("modelBuilder.Entity<MyModel>().HasNoKey();", code);
            Assert.Contains("protected override void OnModelCreating(ModelBuilder modelBuilder)", code);
        }
    }
}
