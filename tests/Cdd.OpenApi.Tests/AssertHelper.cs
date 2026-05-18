namespace Cdd.OpenApi.Tests
{
    public static class AssertHelper
    {
        public static void ContainsNoWhitespace(string expected, string actual)
        {
            var e = System.Text.RegularExpressions.Regex.Replace(expected, @"\s+", "");
            var a = System.Text.RegularExpressions.Regex.Replace(actual, @"\s+", "");
            Xunit.Assert.Contains(e, a);
        }
    }
}
