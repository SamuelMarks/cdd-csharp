using System.Diagnostics;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    public class VersionTest
    {
        [Fact]
        public void VersionCommand_OutputsCorrectVersion()
        {
            var p = new Process();
            p.StartInfo.FileName = "dotnet";
            p.StartInfo.Arguments = $"run --project ../../../../../src/Cdd.OpenApi.Cli --framework net10.0 --version";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Assert.Equal(0, p.ExitCode);
            Assert.Equal("0.0.1\n", output.Replace("\r", ""));
        }
    }
}
