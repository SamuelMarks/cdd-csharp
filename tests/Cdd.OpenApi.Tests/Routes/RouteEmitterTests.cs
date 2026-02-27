using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Routes;
using System.Collections.Generic;

namespace Cdd.OpenApi.Tests.Routes
{
    public class RouteEmitterTests
    {
        [Fact]
        public void ToInterface_ValidPaths_GeneratesCorrectInterface()
        {
            var paths = new OpenApiPaths
            {
                ["/pets"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetPets",
                        Summary = "List all pets",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "limit", In = "query", Schema = new OpenApiSchema { Type = "integer" } }
                        }
                    },
                    Post = new OpenApiOperation
                    {
                        OperationId = "CreatePet"
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IPetsApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.Contains("public interface IPetsApi", code);
            Assert.Contains("/// <summary>", code);
            Assert.Contains("/// List all pets", code);
            Assert.Contains("[HttpGet(\"/pets\")]", code);
            Assert.Contains("void GetPets(int limit);", code);

            Assert.Contains("[HttpPost(\"/pets\")]", code);
            Assert.Contains("void CreatePet();", code);
        }

        [Fact]
        public void ToInterface_GeneratesMethodNameWhenOperationIdMissing()
        {
            var paths = new OpenApiPaths
            {
                ["/store/order"] = new OpenApiPathItem
                {
                    Put = new OpenApiOperation()
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IStoreApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.Contains("void Putstoreorder();", code);
            Assert.Contains("[HttpPut(\"/store/order\")]", code);
        }

        [Fact]
        public void MapTypeToCSharp_MapsNumberToDouble()
        {
            var paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "TestMethod",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "weight", Schema = new OpenApiSchema { Type = "number" } },
                            new OpenApiParameter { Name = "flag", Schema = new OpenApiSchema { Type = "boolean" } },
                            new OpenApiParameter { Name = "unknown", Schema = new OpenApiSchema { Type = "xyz" } }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("ITestApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.Contains("void TestMethod(double weight, bool flag, string unknown);", code);
        }
    }
}
