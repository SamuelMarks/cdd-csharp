using System.Collections.Generic;
using System.Text;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates unit tests for ServerConfiguration.</summary>
    public static class ConfigTestsGenerator
    {
        /// <summary>Generates configuration tests.</summary>
        public static List<GeneratedCode> GenerateConfigTests(string baseNamespace)
        {
            var results = new List<GeneratedCode>();

            var testCode = $@"namespace {baseNamespace}.Tests
{{
    using System;
    using Xunit;
    using {baseNamespace}.Configuration;

    /// <summary>Unit tests for ServerConfiguration.</summary>
    public class ServerConfigurationTests : IDisposable
    {{
        public ServerConfigurationTests()
        {{
            Environment.SetEnvironmentVariable(""DATABASE_URL"", null);
            Environment.SetEnvironmentVariable(""EPHEMERAL_DB"", null);
        }}

        public void Dispose()
        {{
            Environment.SetEnvironmentVariable(""DATABASE_URL"", null);
            Environment.SetEnvironmentVariable(""EPHEMERAL_DB"", null);
        }}

        /// <summary>Tests that configuration parses environment variables correctly.</summary>
        [Fact]
        public void FromEnvironment_ParsesCorrectly()
        {{
            Environment.SetEnvironmentVariable(""DATABASE_URL"", ""Host=localhost"");
            Environment.SetEnvironmentVariable(""EPHEMERAL_DB"", ""true"");

            var config = ServerConfiguration.FromEnvironment();

            Assert.Equal(""Host=localhost"", config.DatabaseUrl);
            Assert.True(config.UseEphemeral);
        }}

        /// <summary>Tests default configuration.</summary>
        [Fact]
        public void FromEnvironment_Defaults()
        {{
            var config = ServerConfiguration.FromEnvironment();

            Assert.Null(config.DatabaseUrl);
            Assert.False(config.UseEphemeral);
        }}
    }}
}}";
            results.Add(new GeneratedCode { FileName = "src/Tests/ServerConfigurationTests.cs", Code = testCode });

            return results;
        }
    }
}
