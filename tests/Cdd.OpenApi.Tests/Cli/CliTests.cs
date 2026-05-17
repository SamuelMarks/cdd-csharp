using System;
using System.IO;
using Xunit;

namespace Cdd.OpenApi.Cli.Tests
{
    [Collection("Cli")]
    public class CliTests : IDisposable
    {
        public CliTests()
        {
            Environment.SetEnvironmentVariable("CDD_COMMAND", null);
            Environment.SetEnvironmentVariable("CDD_ARGS", null);
            Environment.SetEnvironmentVariable("INPUT", null);
            Environment.SetEnvironmentVariable("INPUT_DIR", null);
            Environment.SetEnvironmentVariable("OUTPUT_DIR", null);
            Environment.SetEnvironmentVariable("INPUT_FILE", null);
            Environment.SetEnvironmentVariable("OUTPUT_FILE", null);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("CDD_COMMAND", null);
            Environment.SetEnvironmentVariable("CDD_ARGS", null);
            Environment.SetEnvironmentVariable("INPUT", null);
            Environment.SetEnvironmentVariable("INPUT_DIR", null);
            Environment.SetEnvironmentVariable("OUTPUT_DIR", null);
            Environment.SetEnvironmentVariable("INPUT_FILE", null);
            Environment.SetEnvironmentVariable("OUTPUT_FILE", null);
        }

        [Fact]
        public void HelpCommand_Returns0()
        {
            Assert.Equal(0, Program.Main(new[] { "--help" }));
            Assert.Equal(0, Program.Main(new[] { "-h" }));
            Assert.Equal(0, Program.Main(new[] { "help" }));
        }

        [Fact]
        public void VersionCommand_Returns0()
        {
            Assert.Equal(0, Program.Main(new[] { "--version" }));
            Assert.Equal(0, Program.Main(new[] { "-v" }));
            Assert.Equal(0, Program.Main(new[] { "version" }));
        }

        [Fact]
        public void NoArgs_UsesCddArgsFile()
        {
            File.WriteAllLines(".cdd_args", new[] { "--version" });
            try
            {
                Assert.Equal(0, Program.Main(new string[0]));
            }
            finally
            {
                File.Delete(".cdd_args");
            }
        }

        [Fact]
        public void NoArgs_UsesEnvVars()
        {
            Environment.SetEnvironmentVariable("CDD_COMMAND", "--version");
            Assert.Equal(0, Program.Main(new string[0]));
            
            Environment.SetEnvironmentVariable("CDD_ARGS", "version");
            Environment.SetEnvironmentVariable("CDD_COMMAND", "");
            Assert.Equal(0, Program.Main(new string[0]));
            
            Environment.SetEnvironmentVariable("CDD_COMMAND", "help");
            Environment.SetEnvironmentVariable("CDD_ARGS", "");
            Assert.Equal(0, Program.Main(new string[0]));

            Environment.SetEnvironmentVariable("CDD_COMMAND", "parse");
            Environment.SetEnvironmentVariable("CDD_ARGS", "nonexistent.json");
            Assert.Equal(1, Program.Main(new string[0]));
        }

        [Fact]
        public void NoArgs_AndNoEnvVars_Returns1()
        {
            Environment.SetEnvironmentVariable("CDD_COMMAND", null);
            Environment.SetEnvironmentVariable("CDD_ARGS", null);
            Assert.Equal(1, Program.Main(new string[0]));
        }

        [Fact]
        public void UnknownCommand_Returns1()
        {
            Assert.Equal(1, Program.Main(new[] { "unknown_command" }));
        }

        [Fact]
        public void ParseCommand()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "{ \"openapi\": \"3.0.0\", \"paths\": {} }");
            try
            {
                Assert.Equal(0, Program.Main(new[] { "parse", tempFile }));
                Assert.Equal(1, Program.Main(new[] { "parse", "does_not_exist.json" }));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void EmitCommand()
        {
            var tempInput = Path.GetTempFileName();
            var tempOutput = Path.GetTempFileName();
            File.WriteAllText(tempInput, "{ \"openapi\": \"3.0.0\", \"paths\": {} }");
            try
            {
                Assert.Equal(0, Program.Main(new[] { "emit", tempInput, tempOutput }));
                Assert.Equal(1, Program.Main(new[] { "emit", "does_not_exist.json", tempOutput }));
                Assert.Equal(1, Program.Main(new[] { "emit", tempInput })); // missing output path
            }
            finally
            {
                File.Delete(tempInput);
                File.Delete(tempOutput);
            }
        }

        [Fact]
        public void FromOpenApi_GeneratesSdk()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            var tempInput = Path.Combine(tempDir, "spec.json");
            File.WriteAllText(tempInput, "{ \"openapi\": \"3.0.0\", \"paths\": {} }");
            try
            {
                Assert.Equal(0, Program.Main(new[] { "from_openapi", "to_sdk", "-i", tempInput, "-o", tempDir, "--no-github-actions", "--no-installable-package", "--tests" }));
                Assert.Equal(0, Program.Main(new[] { "from_openapi", "to_sdk_cli", "--input", tempInput, "--output", tempDir }));
                Assert.Equal(0, Program.Main(new[] { "from_openapi", "to_server", "--input-dir", tempDir, "--output", tempDir }));
                Assert.Equal(0, Program.Main(new[] { "from_openapi", "--input", tempInput, "--output", tempDir }));
                
                // missing input
                Assert.Equal(1, Program.Main(new[] { "from_openapi" }));
                
                // exception in handle
                Assert.Equal(1, Program.Main(new[] { "from_openapi", "-i", "does_not_exist.json" }));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
        
        [Fact]
        public void ToOpenApi_Works()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            var tempInput = Path.Combine(tempDir, "Model.cs");
            var tempOutput = Path.Combine(tempDir, "openapi.json");
            File.WriteAllText(tempInput, "public class User { public int Id { get; set; } }");
            try
            {
                Assert.Equal(0, Program.Main(new[] { "to_openapi", "-i", tempInput, "-o", tempOutput }));
                Assert.True(File.Exists(tempOutput));

                Assert.Equal(0, Program.Main(new[] { "to_openapi", "--input", tempDir, "--output", tempOutput }));

                // missing input logic test (default is env vars)
                Environment.SetEnvironmentVariable("INPUT_FILE", null);
                Assert.Equal(1, Program.Main(new[] { "to_openapi" }));
                
                Assert.Equal(1, Program.Main(new[] { "to_openapi", "-i", "does_not_exist.cs" }));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
        
        [Fact]
        public void ToDocsJson_Works()
        {
            var tempInput = Path.GetTempFileName();
            var tempOutput = Path.GetTempFileName();
            File.WriteAllText(tempInput, "{ \"openapi\": \"3.0.0\", \"paths\": {} }");
            try
            {
                Assert.Equal(0, Program.Main(new[] { "to_docs_json", "-i", tempInput, "-o", tempOutput, "--no-imports", "--no-wrapping" }));
                Assert.True(File.Exists(tempOutput));
                
                Assert.Equal(0, Program.Main(new[] { "to_docs_json", "--input", tempInput }));

                Environment.SetEnvironmentVariable("INPUT_FILE", null);
                Assert.Equal(1, Program.Main(new[] { "to_docs_json" }));

                Assert.Equal(1, Program.Main(new[] { "to_docs_json", "-i", "does_not_exist.json" }));
            }
            finally
            {
                File.Delete(tempInput);
                File.Delete(tempOutput);
            }
        }
        [Fact]
        public void ServerJsonRpc_CatchesException()
        {
            // Port "abc" will cause HttpListener to throw an exception and it will be caught in Main or inside the method
            Assert.Equal(1, Program.Main(new[] { "server_json_rpc", "--port", "abc" }));
        }

        [Fact]
        public void ParseCommand_ThrowsOnDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                // File.ReadAllText on a directory will throw UnauthorizedAccessException
                Assert.Equal(1, Program.Main(new[] { "parse", tempDir }));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ToDocsJson_HttpThrows()
        {
            // Http request to a non-existent port should return {} and succeed.
            Assert.Equal(0, Program.Main(new[] { "to_docs_json", "-i", "http://localhost:12345/doesnotexist.json" }));
        }
    }
}
