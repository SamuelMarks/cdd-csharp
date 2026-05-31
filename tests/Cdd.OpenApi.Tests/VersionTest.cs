using System;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    public class VersionTest
    {
        [Fact]
        public void VersionCommand_OutputsCorrectVersion()
        {
            var sw = new System.IO.StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);
            try
            {
                var exitCode = Cdd.OpenApi.Cli.Program.Main(new[] { "--version" });
                Assert.Equal(0, exitCode);
                Assert.Equal("0.0.1\n", sw.ToString().Replace("\r", ""));
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
