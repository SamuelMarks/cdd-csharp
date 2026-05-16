using System;
using Xunit;

namespace Cdd.OpenApi.Cli.Tests
{
    public class CliTests
    {
        [Fact]
        public void HelpCommand_Returns0()
        {
            var result = Program.Main(new[] { "--help" });
            Assert.Equal(0, result);
        }
    }
}
