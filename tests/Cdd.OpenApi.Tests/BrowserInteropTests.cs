// <copyright file="BrowserInteropTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json;
using Xunit;
using Cdd.OpenApi.Cli;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises browser interop tests.</summary>
    public class BrowserInteropTests
    {
        /// <summary>Verifies or exercises from open api valid spec returns json dictionary.</summary>
        [Fact]
        public void FromOpenApi_ValidSpec_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_sdk");
            Assert.Contains("Client.cs", result);
        }

        /// <summary>Verifies or exercises from open api all target returns json dictionary.</summary>
        [Fact]
        public void FromOpenApi_AllTarget_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "all");
            Assert.Contains("Attributes.cs", result);
        }

        /// <summary>Verifies or exercises from open api sdk cli target returns json dictionary.</summary>
        [Fact]
        public void FromOpenApi_SdkCliTarget_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_sdk_cli");
            Assert.Contains("Attributes.cs", result);
        }

        /// <summary>Verifies or exercises from open api sdk cli target with mcp flag returns mcp adapter.</summary>
        [Fact]
        public void FromOpenApi_SdkCliWithMcpFlag_ReturnsMcpFiles()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi --mcp", "to_sdk_cli");
            Assert.Contains("Mcp", result);
        }

        /// <summary>Verifies or exercises from open api server target returns json dictionary.</summary>
        [Fact]
        public void FromOpenApi_ServerTarget_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_server");
            Assert.Contains("Attributes.cs", result);
        }

        /// <summary>Verifies or exercises from open api invalid spec returns error json.</summary>
        [Fact]
        public void FromOpenApi_InvalidSpec_ReturnsErrorJson()
        {
            string spec = "invalid json";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_sdk");
            Assert.Contains("error", result);
        }

        /// <summary>Verifies or exercises from open api with tests returns integration tests.</summary>
        [Fact]
        public void FromOpenApi_WithTests_ReturnsIntegrationTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi --tests", "to_sdk");
            Assert.Contains("tests/IntegrationTests.cs", result);
        }

        /// <summary>Verifies or exercises from open api with generate tests true returns integration tests.</summary>
        [Fact]
        public void FromOpenApi_WithGenerateTestsTrue_ReturnsIntegrationTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_sdk", true);
            Assert.Contains("tests/IntegrationTests.cs", result);
        }

        /// <summary>Verifies or exercises from open api null command no tests.</summary>
        [Fact]
        public void FromOpenApi_NullCommand_NoTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, null!, "to_sdk");
            Assert.Contains("Client.cs", result);
        }

        /// <summary>Verifies or exercises from open api with tests target server no integration tests.</summary>
        [Fact]
        public void FromOpenApi_WithTests_TargetServer_NoIntegrationTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi --tests", "to_server");
            Assert.DoesNotContain("tests/IntegrationTests.cs", result);
        }

        /// <summary>Verifies or exercises from open api with tests target all returns integration tests.</summary>
        [Fact]
        public void FromOpenApi_WithTests_TargetAll_ReturnsIntegrationTests()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi --tests", "all");
            Assert.Contains("tests/IntegrationTests.cs", result);
        }

        /// <summary>Verifies or exercises generate from open api delegates to from open api.</summary>
        [Fact]
        public void GenerateFromOpenApi_ValidSpec_ReturnsJsonDictionary()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.GenerateFromOpenApi(spec, "from_openapi", "to_sdk");
            Assert.Contains("Client.cs", result);
        }

        /// <summary>Verifies or exercises from open api target to docs json returns docs json.</summary>
        [Fact]
        public void FromOpenApi_TargetToDocsJson_ReturnsDocsJson()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "from_openapi", "to_docs_json");
            Assert.Contains("getTest", result);
        }

        /// <summary>Verifies or exercises from open api command to docs json with flags returns docs json.</summary>
        [Fact]
        public void FromOpenApi_CommandToDocsJsonWithFlags_ReturnsDocsJson()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, "to_docs_json --no-imports --no-wrapping", "to_sdk");
            Assert.Contains("getTest", result);
        }

        /// <summary>Verifies or exercises from open api target to docs json with null command returns docs json.</summary>
        [Fact]
        public void FromOpenApi_TargetToDocsJson_NullCommand_ReturnsDocsJson()
        {
            string spec = "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}";
            var result = BrowserInterop.FromOpenApi(spec, null!, "to_docs_json");
            Assert.Contains("getTest", result);
        }

        /// <summary>Verifies or exercises from open api target to openapi returns openapi json dictionary.</summary>
        [Fact]
        public void FromOpenApi_TargetToOpenApi_ReturnsOpenApiJson()
        {
            string code = "namespace Generated { public class PetsController { [HttpGet(\"/pets\")] public void ListPets() {} } }";
            var result = BrowserInterop.FromOpenApi(code, "from_openapi", "to_openapi");
            Assert.Contains("openapi.json", result);
        }

        /// <summary>Verifies or exercises from open api command to openapi returns openapi json dictionary.</summary>
        [Fact]
        public void FromOpenApi_CommandToOpenApi_ReturnsOpenApiJson()
        {
            string code = "namespace Generated { public class PetsController { [HttpGet(\"/pets\")] public void ListPets() {} } }";
            var result = BrowserInterop.FromOpenApi(code, "to_openapi", "to_sdk");
            Assert.Contains("openapi.json", result);
        }

        /// <summary>Verifies or exercises to open api with null input returns error json.</summary>
        [Fact]
        public void ToOpenApi_NullCode_ReturnsErrorJson()
        {
            var result = BrowserInterop.ToOpenApi(null!);
            Assert.Contains("error", result);
        }
    }
}
