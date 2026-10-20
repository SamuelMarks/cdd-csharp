// <copyright file="VersionTest.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises version test.</summary>
    /// <param name=""Cli"">The "Cli" parameter.</param>
    /// <returns>The resulting value or task.</returns>
    [Collection("Cli")]
    public class VersionTest
    {
        /// <summary>Verifies or exercises version command outputs correct version.</summary>
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
                Assert.Equal("0.0.4\n", sw.ToString().Replace("\r", ""));
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
