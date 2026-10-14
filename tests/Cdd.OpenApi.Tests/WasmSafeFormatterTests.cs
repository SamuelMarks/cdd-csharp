using Xunit;
using Microsoft.CodeAnalysis.CSharp;

namespace Cdd.OpenApi.Tests
{
    public class WasmSafeFormatterTests
    {
        [Fact]
        public void TestFormatter()
        {
            var code = "[System.Serializable] class A { int[] B { get; set; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var result = WasmSafeFormatter.Format(tree.GetRoot());
            Assert.Contains("class", result);
            Assert.Contains("int[]", result);
            Assert.Contains("[System.Serializable]", result);
        }
    }
}
