// <copyright file="WasmSafeRoslynTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises wasm safe roslyn tests.</summary>
    public class WasmSafeRoslynTests
    {
        /// <summary>Verifies or exercises parse member.</summary>
        /// <typeparam name="T">The T type parameter.</typeparam>
        /// <param name="code">The code parameter.</param>
        /// <returns>The resulting value or task.</returns>
        private T ParseMember<T>(string code) where T : MemberDeclarationSyntax
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            return tree.GetRoot().DescendantNodes().OfType<T>().First();
        }

        /// <summary>Verifies or exercises get descendant nodes safe returns expected nodes.</summary>
        [Fact]
        public void GetDescendantNodesSafe_ReturnsExpectedNodes()
        {
            var tree = CSharpSyntaxTree.ParseText("class C { public void M() { } }");
            var root = tree.GetRoot();
            var nodes = root.GetDescendantNodesSafe().ToList();
            Assert.Contains(nodes, n => n is ClassDeclarationSyntax);
            Assert.Contains(nodes, n => n is MethodDeclarationSyntax);
        }

        /// <summary>Verifies or exercises format constructor safe expression body formats correctly.</summary>
        [Fact]
        public void FormatConstructorSafe_ExpressionBody_FormatsCorrectly()
        {
            var ctor = ParseMember<ConstructorDeclarationSyntax>("class C { public C() => System.Console.WriteLine(1); }");
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(SyntaxFactory.ClassDeclaration("C").AddMembers(ctor)));
            Assert.Contains("=> System.Console.WriteLine(1);", result);
        }

        /// <summary>Verifies or exercises format constructor safe no body formats correctly.</summary>
        [Fact]
        public void FormatConstructorSafe_NoBody_FormatsCorrectly()
        {
            var ctor = ParseMember<ConstructorDeclarationSyntax>("class C { extern C(); }");
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(SyntaxFactory.ClassDeclaration("C").AddMembers(ctor)));
            Assert.Contains("extern C()", result);
            Assert.Contains(";", result);
        }

        /// <summary>Verifies or exercises format property safe expression body formats correctly.</summary>
        [Fact]
        public void FormatPropertySafe_ExpressionBody_FormatsCorrectly()
        {
            var prop = ParseMember<PropertyDeclarationSyntax>("class C { public int P => 1; }");
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(SyntaxFactory.ClassDeclaration("C").AddMembers(prop)));
            Assert.Contains("=> 1;", result);
        }

        /// <summary>Verifies or exercises format property safe no accessor list no expression body formats correctly.</summary>
        [Fact]
        public void FormatPropertySafe_NoAccessorList_NoExpressionBody_FormatsCorrectly()
        {
            var prop = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "P").WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(SyntaxFactory.ClassDeclaration("C").AddMembers(prop)));
            Assert.Contains(";", result);
        }

        /// <summary>Verifies or exercises format member safe unknown member returns formatted.</summary>
        [Fact]
        public void FormatMemberSafe_UnknownMember_ReturnsFormatted()
        {
            var del = ParseMember<DelegateDeclarationSyntax>("public delegate void D();");
            var result = WasmSafeRoslyn.FormatSafe(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("N")).AddMembers(del));
            Assert.Contains("public delegate void D();", result);
        }
        /// <summary>Verifies or exercises wasm safe formatter extra closing braces does not crash.</summary>
        [Fact]
        public void WasmSafeFormatter_ExtraClosingBraces_DoesNotCrash()
        {
            string code = "public class A { } } }";
            var node = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseCompilationUnit(code);
            string formatted = Cdd.OpenApi.WasmSafeFormatter.Format(node);
            Assert.Contains("}", formatted);
        }

        /// <summary>Verifies or exercises wasm safe roslyn formats attributes and expression bodies.</summary>
        [Fact]
        public void WasmSafeRoslyn_FormatsAttributesAndExpressionBodies()
        {
            string code = @"
// Leading file comment
namespace N {
    [Serializable]
    public class C<T> where T : class {
        [Obsolete]
        private int _f;

        [Obsolete]
        public C() {}

        public int M<U>() where U : struct => 1;
    }

    [Obsolete]
    public interface I : IBase {}
}
";
            var ns = ParseMember<Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax>(code);
            var result = WasmSafeRoslyn.FormatSafe(ns);
            Assert.Contains("// Leading file comment", result);
            Assert.Contains("[Serializable]", result);
            Assert.Contains("[Obsolete]", result);
            Assert.Contains("=> 1;", result);
            Assert.Contains(": IBase", result);

            string fileScopedCode = @"
namespace N.FileScoped;
using System;
/// <summary>Record doc</summary>
[Serializable]
public record R : IBase { public int X { get; set; } }
[Serializable]
public record RNoBase { }
/// <summary>Struct doc</summary>
[Serializable]
public struct S : IBase { public int Y; }
[Serializable]
public struct SNoBase { }
/// <summary>Enum doc</summary>
[Flags]
public enum E { A, B }
";
            var fsNs = ParseMember<Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax>(fileScopedCode);
            var fsResult = WasmSafeRoslyn.FormatSafe(fsNs);
            Assert.Contains("namespace N.FileScoped;", fsResult);
            Assert.Contains("public record R", fsResult);
            Assert.Contains("public record RNoBase", fsResult);
            Assert.Contains("public struct S", fsResult);
            Assert.Contains("public struct SNoBase", fsResult);
            Assert.Contains("public enum E", fsResult);
        }
    }
}
