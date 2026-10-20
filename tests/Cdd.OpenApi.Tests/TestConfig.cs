// <copyright file="TestConfig.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using Xunit;

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises cli collection definition.</summary>
    /// <param name=""Cli"">The "Cli" parameter.</param>
    /// <param name="DisableParallelization">The DisableParallelization parameter.</param>
    /// <returns>The resulting value or task.</returns>
    [CollectionDefinition("Cli", DisableParallelization = true)]
    public class CliCollectionDefinition
    {
    }

    /// <summary>Verifies or exercises test config.</summary>
    public static class TestConfig
    {
        /// <summary>Verifies or exercises tf.</summary>
        public static string TF = "net10.0";
    }
}
