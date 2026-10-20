// <copyright file="OpenApiSchemaExtensionsTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json;
using Cdd.OpenApi.Models;
using Xunit;

namespace Cdd.OpenApi.Tests.Models
{
    /// <summary>Verifies or exercises open api schema extensions tests.</summary>
    public class OpenApiSchemaExtensionsTests
    {
        /// <summary>Verifies or exercises extended schema serialize deserialize.</summary>
        [Fact]
        public void ExtendedSchema_Serialize_Deserialize()
        {
            var schema = new OpenApiSchema
            {
                If = new OpenApiSchema { Type = "string" },
                Then = new OpenApiSchema { MaxLength = 10 },
                Else = new OpenApiSchema { MinLength = 5 },
                Const = "hello",
                Deprecated = true,
                Contains = new OpenApiSchema { Type = "integer" },
                MinContains = 1,
                MaxContains = 5,
                DependentRequired = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IList<string>>
                {
                    { "name", new[] { "age" } }
                }
            };
            var json = JsonSerializer.Serialize(schema);
            var result = JsonSerializer.Deserialize<OpenApiSchema>(json);

            Assert.NotNull(result);
            Assert.NotNull(result.If);
            Assert.Equal("string", result.If.Type);
            Assert.Equal("hello", result.Const?.ToString());
            Assert.True(result.Deprecated);
            Assert.NotNull(result.DependentRequired);
            Assert.True(result.DependentRequired.ContainsKey("name"));
        }
    }
}
