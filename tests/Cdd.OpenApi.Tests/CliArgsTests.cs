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
            Assert.True(File.Exists(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Tests", "DefaultApiTests.cs")));
            Assert.True(File.Exists(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Mocks", "DefaultApiMock.cs")));

            var testsContent = File.ReadAllText(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Tests", "DefaultApiTests.cs"));
            Assert.Contains("IDefaultApi", testsContent);
            Assert.Contains("_api", testsContent);

            var mockContent = File.ReadAllText(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Mocks", "DefaultApiMock.cs"));
            Assert.Contains("IDefaultApi", mockContent);

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
            Assert.True(File.Exists(Path.Combine(tmpDir, "src", "GeneratedProject", "src", "Client", "DefaultApiClient.cs")));

            Directory.Delete(tmpDir, true);
        }

        [Fact]
        public void FromOpenApi_ToSdkCli_ValidArgs_GeneratesCode()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_from_sdk_cli");
            Directory.CreateDirectory(tmpDir);

            var specPath = Path.Combine(tmpDir, "spec.json");
            File.WriteAllText(specPath, "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}");

            var (exitCode, output) = RunMain(new[] { "from_openapi", "to_sdk_cli", "-i", specPath, "-o", tmpDir, "--mcp" });

            Assert.Equal(0, exitCode);
            Assert.Contains("Successfully generated C# code", output);

            Directory.Delete(tmpDir, true);
        }

        [Fact]
        public void FromOpenApi_ToServer_ValidArgs_GeneratesCode()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_from_server");
            Directory.CreateDirectory(tmpDir);

            var specPath = Path.Combine(tmpDir, "spec.json");
            File.WriteAllText(specPath, "{\"openapi\":\"3.2.0\",\"paths\":{\"/test\":{\"get\":{\"operationId\":\"getTest\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}");

            var (exitCode, output) = RunMain(new[] { "from_openapi", "to_server", "-i", specPath, "-o", tmpDir, "--no-github-actions", "--no-installable-package" });

            Assert.Equal(0, exitCode);
            Assert.Contains("Successfully generated C# code", output);

            Directory.Delete(tmpDir, true);
        }

        [Fact]
        public void FromOpenApi_EnvVars_ValidArgs_GeneratesCode()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_env");
            var inDir = Path.Combine(tmpDir, "in");
            var outDir = Path.Combine(tmpDir, "out");
            Directory.CreateDirectory(inDir);
            Directory.CreateDirectory(outDir);

            var specPath = Path.Combine(inDir, "spec.json");
            File.WriteAllText(specPath, "{\"openapi\":\"3.2.0\",\"paths\":{\"/empty\":{}},\"info\":{\"title\":\"\",\"version\":\"\"}}");

            Environment.SetEnvironmentVariable("CDD_INPUT_DIR", inDir);
            Environment.SetEnvironmentVariable("CDD_OUTPUT", outDir);
            Environment.SetEnvironmentVariable("CDD_MCP", "true");
            Environment.SetEnvironmentVariable("MCP", "true");

            try
            {
                var (exitCode, output) = RunMain(new[] { "from_openapi" });
                Assert.Equal(0, exitCode);
                Assert.Contains("Successfully generated C# code", output);
            }
            finally
            {
                Environment.SetEnvironmentVariable("CDD_INPUT_DIR", null);
                Environment.SetEnvironmentVariable("CDD_OUTPUT", null);
                Environment.SetEnvironmentVariable("CDD_MCP", null);
                Environment.SetEnvironmentVariable("MCP", null);
                Directory.Delete(tmpDir, true);
            }
        }

        [Fact]
        public void FromOpenApi_TagsAndAdditionalOps_GeneratesCode()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_tags");
            Directory.CreateDirectory(tmpDir);

            var specPath = Path.Combine(tmpDir, "spec.json");
            File.WriteAllText(specPath, "{\"openapi\":\"3.2.0\",\"paths\":{\"/empty\":{}, \"/tagged\":{\"get\":{\"tags\":[\"\"]}, \"x-custom\":{\"operationId\":\"customOp\"}}},\"info\":{\"title\":\"\",\"version\":\"\"}}");

            var (exitCode, output) = RunMain(new[] { "from_openapi", "-i", specPath, "-o", tmpDir });

            Assert.Equal(0, exitCode);
            Assert.Contains("Successfully generated C# code", output);

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
        [Fact]
        public void SyncCommand_MissingTruth_Returns1()
        {
            var (exitCode, output) = RunMain(new[] { "sync" });
            // Assert output could be captured via Console.Error, but RunMain only captures Console.Out.
            // We just check the exit code.
            Assert.Equal(1, exitCode);
        }

        [Fact]
        public void SyncCommand_UnsupportedTruth_Returns1()
        {
            var (exitCode, output) = RunMain(new[] { "sync", "--truth", "unsupported", "-i", "in", "-o", "out" });
            Assert.Equal(1, exitCode);
        }

        [Fact]
        public void SyncCommand_IncompleteArgs_Ignored()
        {
            var (exitCode, output) = RunMain(new[] { "sync", "--truth" });
            Assert.Equal(1, exitCode); // Because truth remains empty as i+1 >= args.Length
            var (exitCode2, output2) = RunMain(new[] { "sync", "--truth", "class", "-i" });
            // Runs but fails because there's no input provided
            var (exitCode3, output3) = RunMain(new[] { "sync", "--truth", "class", "-i", "in", "-o" });
        }

        [Fact]
        public void SyncCommand_ValidArgs_CallsToOpenApi()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_sync");
            Directory.CreateDirectory(tmpDir);

            var codePath = Path.Combine(tmpDir, "Model.cs");
            File.WriteAllText(codePath, "public class User {}");

            var outPath = Path.Combine(tmpDir, "spec.json");

            var (exitCode, output) = RunMain(new[] { "sync", "--truth", "class", "-i", tmpDir, "-o", outPath });

            Assert.Equal(0, exitCode);
            Assert.Contains("Synchronizing from truth", output);
            Assert.Contains("Successfully generated spec", output);
            Assert.True(File.Exists(outPath));

            var outPathFunction = Path.Combine(tmpDir, "spec_function.json");
            var (exitCodeFunction, outputFunction) = RunMain(new[] { "sync", "--truth", "function", "--input", tmpDir, "--output", outPathFunction });
            Assert.Equal(0, exitCodeFunction);
            Assert.Contains("Synchronizing from truth", outputFunction);
            Assert.Contains("Successfully generated spec", outputFunction);
            Assert.True(File.Exists(outPathFunction));

            try { Directory.Delete(tmpDir, true); } catch { }
        }
    }
}
