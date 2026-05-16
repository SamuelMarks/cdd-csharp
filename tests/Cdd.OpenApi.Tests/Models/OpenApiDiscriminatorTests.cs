using System.Text.Json;
using Cdd.OpenApi.Models;
using Xunit;

namespace Cdd.OpenApi.Tests.Models
{
    public class OpenApiDiscriminatorTests
    {
        [Fact]
        public void Discriminator_Serialize_Deserialize()
        {
            var disc = new OpenApiDiscriminator
            {
                PropertyName = "petType",
                Mapping = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "dog", "#/components/schemas/Dog" }
                }
            };
            var json = JsonSerializer.Serialize(disc);
            var result = JsonSerializer.Deserialize<OpenApiDiscriminator>(json);

            Assert.NotNull(result);
            Assert.Equal("petType", result.PropertyName);
            Assert.NotNull(result.Mapping);
            Assert.Equal("#/components/schemas/Dog", result.Mapping["dog"]);
        }

        [Fact]
        public void Discriminator_Serialize_Deserialize_Swagger2()
        {
            var disc = new OpenApiDiscriminator
            {
                PropertyName = "petType"
            };
            var json = JsonSerializer.Serialize(disc);
            Assert.Equal("\"petType\"", json);

            var result = JsonSerializer.Deserialize<OpenApiDiscriminator>(json);
            Assert.NotNull(result);
            Assert.Equal("petType", result.PropertyName);
            Assert.Null(result.Mapping);
            Assert.Null(result.DefaultMapping);
        }

        [Fact]
        public void Discriminator_Serialize_Deserialize_DefaultMapping()
        {
            var disc = new OpenApiDiscriminator
            {
                PropertyName = "petType",
                DefaultMapping = "#/components/schemas/Pet"
            };
            var json = JsonSerializer.Serialize(disc);
            var result = JsonSerializer.Deserialize<OpenApiDiscriminator>(json);

            Assert.NotNull(result);
            Assert.Equal("petType", result.PropertyName);
            Assert.Equal("#/components/schemas/Pet", result.DefaultMapping);
            Assert.Null(result.Mapping);
        }

        [Fact]
        public void Discriminator_Deserialize_InvalidToken_Throws()
        {
            var json = "123";
            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OpenApiDiscriminator>(json));
            Assert.Equal("Expected string or object for Discriminator.", ex.Message);
        }

        [Fact]
        public void Schema_CanContainDiscriminator()
        {
            var schema = new OpenApiSchema
            {
                Discriminator = new OpenApiDiscriminator { PropertyName = "type" }
            };
            var json = JsonSerializer.Serialize(schema);
            Assert.Contains("discriminator", json);
            Assert.Contains("type", json);

            var result = JsonSerializer.Deserialize<OpenApiSchema>(json);
            Assert.NotNull(result?.Discriminator);
            Assert.Equal("type", result.Discriminator.PropertyName);
        }
    }
}
