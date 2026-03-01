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