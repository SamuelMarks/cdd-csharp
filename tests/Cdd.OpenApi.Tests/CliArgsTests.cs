using System.IO;
using Xunit;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

namespace Cdd.OpenApi.Tests
{
    public class CliArgsTests
    {
        // Simple manual validation since the CLI directly executes logic
        // We will execute the compiled CLI process directly to ensure it works end-to-end
        
        [Fact]
        public void FromOpenApi_ValidArgs_GeneratesCode()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), "cli_test_from");
            Directory.CreateDirectory(tmpDir);
            
            var specPath = Path.Combine(tmpDir, "spec.json");
            File.WriteAllText(specPath, "{\"openapi\":\"3.2.0\",\"paths\":{},\"info\":{\"title\":\"\",\"version\":\"\"}}");
            
            var p = new Process();
            p.StartInfo.FileName = "dotnet";
            p.StartInfo.Arguments = $"run --project ../../../../../src/Cdd.OpenApi.Cli from_openapi -i {specPath} -o {tmpDir}";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            
            Assert.Equal(0, p.ExitCode);
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
            
            var p = new Process();
            p.StartInfo.FileName = "dotnet";
            p.StartInfo.Arguments = $"run --project ../../../../../src/Cdd.OpenApi.Cli to_openapi -i {tmpDir} -o {outPath}";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            
            Assert.Equal(0, p.ExitCode);
            Assert.Contains("Successfully generated spec", output);
            Assert.True(File.Exists(outPath));
            
            Directory.Delete(tmpDir, true);
        }
    }
}
