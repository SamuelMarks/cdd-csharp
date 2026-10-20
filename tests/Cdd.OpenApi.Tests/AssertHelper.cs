// <copyright file="AssertHelper.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises assert helper.</summary>
    public static class AssertHelper
    {
        /// <summary>Verifies or exercises contains no whitespace.</summary>
        /// <param name="expected">The expected parameter.</param>
        /// <param name="actual">The actual parameter.</param>
        public static void ContainsNoWhitespace(string expected, string actual)
        {
            var e = System.Text.RegularExpressions.Regex.Replace(expected, @"\s+", "");
            var a = System.Text.RegularExpressions.Regex.Replace(actual, @"\s+", "");
            Xunit.Assert.Contains(e, a);
        }
    }
}
