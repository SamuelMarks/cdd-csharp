using System;
using System.IO;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    [Collection("Cli")]
    public class CliArgsTests
    {
        private (int ExitCode, string Output) RunMain(string[] args)
        {
            var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);
            try
            {
                var exitCode = Cdd.OpenApi.Cli.Program.Main(args);
                return (exitCode, sw.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void FromOpenApi_ValidArgs_GeneratesCode()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_from");
            Directory.CreateDirectory(tmpDir);

            var specPath = Path.Combine(tmpDir, "spec.json");
            File.WriteAllText(specPath, "{\"openapi\":\"3.2.0\",\"paths\":{},\"info\":{\"title\":\"\",\"version\":\"\"}}");

            var (exitCode, output) = RunMain(new[] { "from_openapi", "-i", specPath, "-o", tmpDir });

            Assert.Equal(0, exitCode);
            Assert.Contains("Successfully generated C# code", output);

            Directory.Delete(tmpDir, true);
        }

        [Fact]
        public void FromOpenApi_CreateComposableTestsAndMocks_GeneratesComposableCode()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_composable");
            Directory.CreateDirectory(tmpDir);

            var specPath = Path.Combine(tmpDir, "spec.json");
            File.WriteAllText(specPath, "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}");

            var (exitCode, output) = RunMain(new[] { "from_openapi", "-i", specPath, "-o", tmpDir, "--tests" });

            Assert.Equal(0, exitCode);
            Assert.Contains("Successfully generated C# code", output);
            Assert.True(File.Exists(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Tests", "ApiTests.cs")));
            Assert.True(File.Exists(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Mocks", "ApiMock.cs")));

            var testsContent = File.ReadAllText(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Tests", "ApiTests.cs"));
            Assert.Contains("IApi", testsContent);
            Assert.Contains("_api", testsContent);

            var mockContent = File.ReadAllText(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Mocks", "ApiMock.cs"));
            Assert.Contains("IApi", mockContent);

            Directory.Delete(tmpDir, true);
        }

        [Fact]
        public void FromOpenApi_ToSdk_ValidArgs_GeneratesCode()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_from_sdk");
            Directory.CreateDirectory(tmpDir);

            var specPath = Path.Combine(tmpDir, "spec.json");
            File.WriteAllText(specPath, "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}");

            var (exitCode, output) = RunMain(new[] { "from_openapi", "to_sdk", "-i", specPath, "-o", tmpDir });

            Assert.Equal(0, exitCode);
            Assert.Contains("Successfully generated C# code", output);
            Assert.True(File.Exists(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Client", "Client.cs")));

            Directory.Delete(tmpDir, true);
        }

        [Fact]
        public void ToOpenApi_ValidArgs_GeneratesSpec()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_to");
            Directory.CreateDirectory(tmpDir);

            var codePath = Path.Combine(tmpDir, "Model.cs");
            File.WriteAllText(codePath, "public class User {}");

            var outPath = Path.Combine(tmpDir, "spec.json");

            var (exitCode, output) = RunMain(new[] { "to_openapi", "-i", tmpDir, "-o", outPath });

            Assert.Equal(0, exitCode);
            Assert.Contains("Successfully generated spec", output);
            Assert.True(File.Exists(outPath));

            Directory.Delete(tmpDir, true);
        }
    }
}
