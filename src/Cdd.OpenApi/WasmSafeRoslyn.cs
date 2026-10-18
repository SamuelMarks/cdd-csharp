// <copyright file="WasmSafeRoslyn.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Cdd.OpenApi
{
    /// <summary>
    /// Utility class providing WASM-safe methods for Roslyn syntax analysis.
    /// Used to avoid stack overflows from deep recursion in Roslyn's default walkers when running in WASI/WASM.
    /// </summary>
    public static class WasmSafeRoslyn
    {
        /// <summary>
        /// Iteratively retrieves all descendant nodes, avoiding recursive stack frames.
        /// </summary>
        /// <param name="root">The root syntax node to traverse.</param>
        /// <returns>An enumerable of all descendant syntax nodes.</returns>
        public static IEnumerable<SyntaxNode> GetDescendantNodesSafe(this SyntaxNode root)
        {
            var stack = new Stack<SyntaxNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current != root)
                {
                    yield return current;
                }

                // Push children in reverse order so they are popped in original order
                foreach (var child in current.ChildNodes().Reverse())
                {
                    stack.Push(child);
                }
            }
        }

        /// <summary>
        /// Formats a namespace declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="ns">The namespace declaration syntax node to format.</param>
        /// <returns>The formatted C# namespace code.</returns>
        public static string FormatSafe(this BaseNamespaceDeclarationSyntax ns)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var usingDir in ns.Usings)
            {
                sb.AppendLine(WasmSafeFormatter.Format(usingDir));
            }
            if (ns is FileScopedNamespaceDeclarationSyntax)
            {
                sb.AppendLine($"namespace {WasmSafeFormatter.Format(ns.Name)};");
                foreach (var member in ns.Members)
                {
                    sb.AppendLine(FormatMemberSafe(member));
                }
            }
            else
            {
                sb.AppendLine($"namespace {WasmSafeFormatter.Format(ns.Name)}");
                sb.AppendLine("{");
                foreach (var member in ns.Members)
                {
                    sb.AppendLine(FormatMemberSafe(member));
                }
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Dispatches formatting for a member declaration syntax node to the appropriate specialized formatter.
        /// </summary>
        /// <param name="member">The member declaration syntax node to format.</param>
        /// <returns>The formatted C# member declaration code.</returns>
        private static string FormatMemberSafe(MemberDeclarationSyntax member)
        {
            if (member is ClassDeclarationSyntax classNode) return FormatClassSafe(classNode);
            if (member is InterfaceDeclarationSyntax interfaceNode) return FormatInterfaceSafe(interfaceNode);
            if (member is RecordDeclarationSyntax recordNode) return FormatRecordSafe(recordNode);
            if (member is StructDeclarationSyntax structNode) return FormatStructSafe(structNode);
            if (member is EnumDeclarationSyntax enumNode) return FormatEnumSafe(enumNode);
            if (member is MethodDeclarationSyntax methodNode) return FormatMethodSafe(methodNode);
            if (member is PropertyDeclarationSyntax propNode) return FormatPropertySafe(propNode);
            if (member is FieldDeclarationSyntax fieldNode) return FormatFieldSafe(fieldNode);
            if (member is ConstructorDeclarationSyntax ctorNode) return FormatConstructorSafe(ctorNode);
            return WasmSafeFormatter.Format(member);
        }

        /// <summary>
        /// Appends leading documentation trivia and attribute lists of a syntax node to a <see cref="System.Text.StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The target string builder.</param>
        /// <param name="node">The syntax node whose leading trivia and attributes should be appended.</param>
        /// <param name="attributes">The attribute lists attached to the syntax node.</param>
        private static void AppendLeadingTrivia(System.Text.StringBuilder sb, SyntaxNode node, SyntaxList<AttributeListSyntax> attributes)
        {
            if (node.HasLeadingTrivia)
            {
                foreach (var trivia in node.GetLeadingTrivia())
                {
                    var str = trivia.ToFullString().Trim();
                    if (!string.IsNullOrEmpty(str)) sb.AppendLine(str);
                }
            }
            foreach (var attr in attributes)
            {
                sb.AppendLine(WasmSafeFormatter.Format(attr.WithoutLeadingTrivia()));
            }
        }

        /// <summary>
        /// Formats a record declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="recordNode">The record declaration syntax node to format.</param>
        /// <returns>The formatted C# record declaration code.</returns>
        private static string FormatRecordSafe(RecordDeclarationSyntax recordNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, recordNode, recordNode.AttributeLists);
            var modifiers = string.Join(" ", recordNode.Modifiers.Select(m => m.Text));
            var baseList = recordNode.BaseList != null ? $" : {string.Join(", ", recordNode.BaseList.Types.Select(t => WasmSafeFormatter.Format(t)))}" : "";
            sb.AppendLine($"{modifiers} record {recordNode.Identifier.Text}{baseList}");
            sb.AppendLine("{");
            foreach (var member in recordNode.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Formats a struct declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="structNode">The struct declaration syntax node to format.</param>
        /// <returns>The formatted C# struct declaration code.</returns>
        private static string FormatStructSafe(StructDeclarationSyntax structNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, structNode, structNode.AttributeLists);
            var modifiers = string.Join(" ", structNode.Modifiers.Select(m => m.Text));
            var baseList = structNode.BaseList != null ? $" : {string.Join(", ", structNode.BaseList.Types.Select(t => WasmSafeFormatter.Format(t)))}" : "";
            sb.AppendLine($"{modifiers} struct {structNode.Identifier.Text}{baseList}");
            sb.AppendLine("{");
            foreach (var member in structNode.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Formats an enum declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="enumNode">The enum declaration syntax node to format.</param>
        /// <returns>The formatted C# enum declaration code.</returns>
        private static string FormatEnumSafe(EnumDeclarationSyntax enumNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, enumNode, enumNode.AttributeLists);
            var modifiers = string.Join(" ", enumNode.Modifiers.Select(m => m.Text));
            sb.AppendLine($"{modifiers} enum {enumNode.Identifier.Text}");
            sb.AppendLine("{");
            sb.AppendLine(string.Join("," + System.Environment.NewLine, enumNode.Members.Select(m => WasmSafeFormatter.Format(m))));
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Formats a class declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="classNode">The class declaration syntax node to format.</param>
        /// <returns>The formatted C# class declaration code.</returns>
        private static string FormatClassSafe(ClassDeclarationSyntax classNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, classNode, classNode.AttributeLists);
            var modifiers = string.Join(" ", classNode.Modifiers.Select(m => m.Text));
            var baseList = classNode.BaseList != null ? $" : {string.Join(", ", classNode.BaseList.Types.Select(t => WasmSafeFormatter.Format(t)))}" : "";
            sb.AppendLine($"{modifiers} class {classNode.Identifier.Text}{baseList}");
            sb.AppendLine("{");
            foreach (var member in classNode.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Formats an interface declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="interfaceNode">The interface declaration syntax node to format.</param>
        /// <returns>The formatted C# interface declaration code.</returns>
        private static string FormatInterfaceSafe(InterfaceDeclarationSyntax interfaceNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, interfaceNode, interfaceNode.AttributeLists);
            var modifiers = string.Join(" ", interfaceNode.Modifiers.Select(m => m.Text));
            var baseList = interfaceNode.BaseList != null ? $" : {string.Join(", ", interfaceNode.BaseList.Types.Select(t => WasmSafeFormatter.Format(t)))}" : "";
            sb.AppendLine($"{modifiers} interface {interfaceNode.Identifier.Text}{baseList}");
            sb.AppendLine("{");
            foreach (var member in interfaceNode.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Formats a method declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="methodNode">The method declaration syntax node to format.</param>
        /// <returns>The formatted C# method declaration code.</returns>
        private static string FormatMethodSafe(MethodDeclarationSyntax methodNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, methodNode, methodNode.AttributeLists);
            var modifiers = string.Join(" ", methodNode.Modifiers.Select(m => m.Text));
            var returnType = WasmSafeFormatter.Format(methodNode.ReturnType);
            var parameters = string.Join(", ", methodNode.ParameterList.Parameters.Select(p => WasmSafeFormatter.Format(p)));
            var sig = string.IsNullOrEmpty(modifiers) ? $"{returnType} {methodNode.Identifier.Text}({parameters})" : $"{modifiers} {returnType} {methodNode.Identifier.Text}({parameters})";
            if (methodNode.Body == null && methodNode.ExpressionBody == null) sb.AppendLine($"{sig};");
            else if (methodNode.ExpressionBody != null) sb.AppendLine($"{sig} => {WasmSafeFormatter.Format(methodNode.ExpressionBody.Expression)};");
            else
            {
                sb.AppendLine(sig);
                sb.AppendLine("{");
                foreach (var stmt in methodNode.Body!.Statements) sb.AppendLine(WasmSafeFormatter.Format(stmt));
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Formats a constructor declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="ctorNode">The constructor declaration syntax node to format.</param>
        /// <returns>The formatted C# constructor declaration code.</returns>
        private static string FormatConstructorSafe(ConstructorDeclarationSyntax ctorNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, ctorNode, ctorNode.AttributeLists);
            var modifiers = string.Join(" ", ctorNode.Modifiers.Select(m => m.Text));
            var parameters = string.Join(", ", ctorNode.ParameterList.Parameters.Select(p => WasmSafeFormatter.Format(p)));
            var initializer = ctorNode.Initializer != null ? $" {WasmSafeFormatter.Format(ctorNode.Initializer)}" : "";
            sb.AppendLine($"{modifiers} {ctorNode.Identifier.Text}({parameters}){initializer}");
            if (ctorNode.Body != null)
            {
                sb.AppendLine("{");
                foreach (var stmt in ctorNode.Body.Statements) sb.AppendLine(WasmSafeFormatter.Format(stmt));
                sb.AppendLine("}");
            }
            else if (ctorNode.ExpressionBody != null) sb.AppendLine($"=> {WasmSafeFormatter.Format(ctorNode.ExpressionBody.Expression)};");
            else sb.AppendLine(";");
            return sb.ToString();
        }

        /// <summary>
        /// Formats a property declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="propNode">The property declaration syntax node to format.</param>
        /// <returns>The formatted C# property declaration code.</returns>
        private static string FormatPropertySafe(PropertyDeclarationSyntax propNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, propNode, propNode.AttributeLists);
            var modifiers = string.Join(" ", propNode.Modifiers.Select(m => m.Text));
            var type = WasmSafeFormatter.Format(propNode.Type);
            sb.Append($"{modifiers} {type} {propNode.Identifier.Text}");
            if (propNode.AccessorList != null)
            {
                sb.AppendLine($" {WasmSafeFormatter.Format(propNode.AccessorList)}");
            }
            else if (propNode.ExpressionBody != null) sb.AppendLine($" => {WasmSafeFormatter.Format(propNode.ExpressionBody.Expression)};");
            else sb.AppendLine(";");
            if (propNode.Initializer != null) sb.AppendLine($" = {WasmSafeFormatter.Format(propNode.Initializer.Value)};");
            return sb.ToString();
        }

        /// <summary>
        /// Formats a field declaration syntax node safely without deep recursion.
        /// </summary>
        /// <param name="fieldNode">The field declaration syntax node to format.</param>
        /// <returns>The formatted C# field declaration code.</returns>
        private static string FormatFieldSafe(FieldDeclarationSyntax fieldNode)
        {
            var sb = new System.Text.StringBuilder();
            AppendLeadingTrivia(sb, fieldNode, fieldNode.AttributeLists);
            var modifiers = string.Join(" ", fieldNode.Modifiers.Select(m => m.Text));
            var type = WasmSafeFormatter.Format(fieldNode.Declaration.Type);
            var vars = string.Join(", ", fieldNode.Declaration.Variables.Select(v => WasmSafeFormatter.Format(v)));
            sb.AppendLine($"{modifiers} {type} {vars};");
            return sb.ToString();
        }
    }
}
