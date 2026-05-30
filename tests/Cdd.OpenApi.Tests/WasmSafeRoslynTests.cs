using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    public class WasmSafeRoslynTests
    {
        private T ParseMember<T>(string code) where T : MemberDeclarationSyntax
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            return tree.GetRoot().DescendantNodes().OfType<T>().First();
        }

        [Fact]
        public void FormatConstructorSafe_ExpressionBody_FormatsCorrectly()
        {
            var ctor = ParseMember<ConstructorDeclarationSyntax>("class C { public C() => System.Console.WriteLine(1); }");
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(SyntaxFactory.ClassDeclaration("C").AddMembers(ctor)));
            Assert.Contains("=> System.Console.WriteLine(1);", result);
        }

        [Fact]
        public void FormatConstructorSafe_NoBody_FormatsCorrectly()
        {
            var ctor = ParseMember<ConstructorDeclarationSyntax>("class C { extern C(); }");
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(SyntaxFactory.ClassDeclaration("C").AddMembers(ctor)));
            Assert.Contains("extern C()", result);
            Assert.Contains(";", result);
        }

        [Fact]
        public void FormatPropertySafe_ExpressionBody_FormatsCorrectly()
        {
            var prop = ParseMember<PropertyDeclarationSyntax>("class C { public int P => 1; }");
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(SyntaxFactory.ClassDeclaration("C").AddMembers(prop)));
            Assert.Contains("=> 1;", result);
        }

        [Fact]
        public void FormatPropertySafe_NoAccessorList_NoExpressionBody_FormatsCorrectly()
        {
            var prop = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "P").WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(SyntaxFactory.ClassDeclaration("C").AddMembers(prop)));
            Assert.Contains(";", result);
        }

        [Fact]
        public void FormatMemberSafe_UnknownMember_ReturnsFormatted()
        {
            var del = ParseMember<DelegateDeclarationSyntax>("public delegate void D();");
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(del));
            Assert.Contains("public delegate void D();", result);
        }
    }
}
