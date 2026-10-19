using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Models
{
    public class OpenApiExampleTests
    {
        [Fact]
        public void Example_Properties_CanBeSetAndGot()
        {
            var example = new OpenApiExample
            {
                Summary = "A summary",
                Description = "A description",
                Value = 42,
                ExternalValue = "http://example.com/value.txt"
            };

            Assert.Equal("A summary", example.Summary);
            Assert.Equal("A description", example.Description);
            Assert.Equal(42, example.Value);
            Assert.Equal("http://example.com/value.txt", example.ExternalValue);

            OpenApiAny strAny = "hello";
            OpenApiAny intAny = 42;
            OpenApiAny boolTrue = true;
            OpenApiAny boolFalse = false;
            OpenApiAny schemaAny = new OpenApiSchema { Type = "string" };

            Assert.Equal("hello", strAny.ToString());
            Assert.Equal(strAny.GetHashCode(), ((OpenApiAny)"hello").GetHashCode());
            Assert.True(strAny.Equals((object)(OpenApiAny)"hello"));
            Assert.False(strAny.Equals((object)"not_any"));
            Assert.False(strAny.Equals((OpenApiAny?)null));
            Assert.False(strAny.Equals(intAny));
            Assert.False(strAny.Equals(new OpenApiAny("hello", System.Text.Json.JsonValueKind.Number)));
            Assert.Equal("true", boolTrue.Value);
            Assert.Equal("false", boolFalse.Value);
            Assert.NotNull(schemaAny.Schema);

            var doc = new OpenApiDocument
            {
                Components = new OpenApiComponents
                {
                    Schemas = new System.Collections.Generic.Dictionary<string, OpenApiSchema>
                    {
                        ["Item"] = new OpenApiSchema
                        {
                            Type = "string",
                            Default = "def",
                            Example = 42,
                            Const = true
                        }
                    }
                }
            };
            var json = new Cdd.OpenApi.Emit.OpenApiEmitter().EmitJson(doc);
            var parsed = new Cdd.OpenApi.Parse.OpenApiParser().ParseJson(json);
            var item = parsed.Components!.Schemas!["Item"];
            Assert.Equal((OpenApiAny)"def", item.Default);
            Assert.Equal((OpenApiAny)42, item.Example);
            Assert.Equal((OpenApiAny)true, item.Const);

            OpenApiAny anyWithSchema1 = new OpenApiSchema { Type = "string" };
            OpenApiAny anyWithSchema2 = new OpenApiSchema { Type = "integer" };
            Assert.True(anyWithSchema1.Equals(anyWithSchema1));
            Assert.False(anyWithSchema1.Equals(anyWithSchema2));
            Assert.False(anyWithSchema1.Equals((object?)null));
            Assert.NotEqual(anyWithSchema1.GetHashCode(), anyWithSchema2.GetHashCode());
        }
    }
}
