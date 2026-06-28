using System.Text.Json;
using Xunit;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Cdd.OpenApi.Tests
{
    public class BrowserInteropTests
    {
        [Fact]
        public void FromOpenApi_ValidSpec_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_sdk");
            Assert.Contains("Client.cs", result);
        }

        [Fact]
        public void FromOpenApi_AllTarget_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "all");
            Assert.Contains("Attributes.cs", result);
        }

        [Fact]
        public void FromOpenApi_SdkCliTarget_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_sdk_cli");
            Assert.Contains("Attributes.cs", result);
        }

        [Fact]
        public void FromOpenApi_ServerTarget_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_server");
            Assert.Contains("Attributes.cs", result);
        }

        [Fact]
        public void FromOpenApi_InvalidSpec_ReturnsErrorJson()
        {
            string spec = "invalid json";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_sdk");
            Assert.Contains("error", result);
        }

        [Fact]
        public void FromOpenApi_WithTests_ReturnsIntegrationTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi --tests", "to_sdk");
            Assert.Contains("tests/IntegrationTests.cs", result);
        }

        [Fact]
        public void FromOpenApi_WithGenerateTestsTrue_ReturnsIntegrationTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_sdk", true);
            Assert.Contains("tests/IntegrationTests.cs", result);
        }

        [Fact]
        public void FromOpenApi_NullCommand_NoTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, null, "to_sdk");
            Assert.Contains("Client.cs", result);
        }

        [Fact]
        public void FromOpenApi_WithTests_TargetServer_NoIntegrationTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi --tests", "to_server");
            Assert.DoesNotContain("tests/IntegrationTests.cs", result);
        }

        [Fact]
        public void FromOpenApi_WithTests_TargetAll_ReturnsIntegrationTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi --tests", "all");
            Assert.Contains("tests/IntegrationTests.cs", result);
        }
    }
}
