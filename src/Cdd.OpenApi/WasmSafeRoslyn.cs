using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Cdd.OpenApi
{
    /// <summary>
    /// Utility class providing WASM-safe methods for Roslyn syntax analysis.
    /// Used to avoid deep recursion limitations of the Mono Interpreter in V8.
    /// </summary>
    public static class WasmSafeRoslyn
    {
        /// <summary>
        /// A flat, non-recursive alternative to `DescendantNodes()` to prevent stack overflows.
        /// </summary>
        public static IEnumerable<SyntaxNode> GetDescendantNodesSafe(this SyntaxNode root)
        {
            var stack = new Stack<SyntaxNode>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current != root) yield return current;
                var children = current.ChildNodesAndTokens();
                foreach (var childOrToken in children.Reverse())
                {
                    if (childOrToken.IsNode) stack.Push(childOrToken.AsNode()!);
                }
            }
        }

        /// <summary>
        /// A basic text formatter that avoids recursive syntax rewrites where possible.
        /// </summary>
        public static string FormatSafe(NamespaceDeclarationSyntax ns)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"namespace {ns.Name}\n{{");
            foreach (var usingDir in ns.Usings) sb.AppendLine(usingDir.NormalizeWhitespace().ToFullString());
            foreach (var member in ns.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string FormatMemberSafe(MemberDeclarationSyntax member)
        {
            if (member is ClassDeclarationSyntax classNode) return FormatClassSafe(classNode);
            if (member is InterfaceDeclarationSyntax interfaceNode) return FormatInterfaceSafe(interfaceNode);
            if (member is MethodDeclarationSyntax methodNode) return FormatMethodSafe(methodNode);
            if (member is PropertyDeclarationSyntax propNode) return FormatPropertySafe(propNode);
            if (member is ConstructorDeclarationSyntax ctorNode) return FormatConstructorSafe(ctorNode);
            if (member is FieldDeclarationSyntax fieldNode) return FormatFieldSafe(fieldNode);
            return member.NormalizeWhitespace().ToFullString();
        }

        private static string FormatClassSafe(ClassDeclarationSyntax classNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in classNode.AttributeLists) sb.AppendLine(attr.NormalizeWhitespace().ToFullString());
            var modifiers = string.Join(" ", classNode.Modifiers.Select(m => m.Text));
            var baseList = classNode.BaseList != null ? $" : {classNode.BaseList.NormalizeWhitespace().ToFullString()}" : "";
            sb.AppendLine($"{modifiers} class {classNode.Identifier.Text}{baseList}\n{{");
            foreach (var member in classNode.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string FormatInterfaceSafe(InterfaceDeclarationSyntax interfaceNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in interfaceNode.AttributeLists) sb.AppendLine(attr.NormalizeWhitespace().ToFullString());
            var modifiers = string.Join(" ", interfaceNode.Modifiers.Select(m => m.Text));
            var baseList = interfaceNode.BaseList != null ? $" : {interfaceNode.BaseList.NormalizeWhitespace().ToFullString()}" : "";
            sb.AppendLine($"{modifiers} interface {interfaceNode.Identifier.Text}{baseList}\n{{");
            foreach (var member in interfaceNode.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string FormatMethodSafe(MethodDeclarationSyntax methodNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in methodNode.AttributeLists) sb.AppendLine(attr.NormalizeWhitespace().ToFullString());
            var modifiers = string.Join(" ", methodNode.Modifiers.Select(m => m.Text));
            var returnType = methodNode.ReturnType.NormalizeWhitespace().ToFullString();
            var parameters = methodNode.ParameterList.NormalizeWhitespace().ToFullString();
            sb.AppendLine($"{modifiers} {returnType} {methodNode.Identifier.Text}{parameters}");
            if (methodNode.Body != null)
            {
                sb.AppendLine("{");
                foreach (var stmt in methodNode.Body.Statements) sb.AppendLine(stmt.NormalizeWhitespace().ToFullString());
                sb.AppendLine("}");
            }
            else if (methodNode.ExpressionBody != null) sb.AppendLine($"=> {methodNode.ExpressionBody.Expression.NormalizeWhitespace().ToFullString()};");
            else sb.AppendLine(";");
            return sb.ToString();
        }

        private static string FormatConstructorSafe(ConstructorDeclarationSyntax ctorNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in ctorNode.AttributeLists) sb.AppendLine(attr.NormalizeWhitespace().ToFullString());
            var modifiers = string.Join(" ", ctorNode.Modifiers.Select(m => m.Text));
            var parameters = ctorNode.ParameterList.NormalizeWhitespace().ToFullString();
            var init = ctorNode.Initializer != null ? $" : {ctorNode.Initializer.NormalizeWhitespace().ToFullString()}" : "";
            sb.AppendLine($"{modifiers} {ctorNode.Identifier.Text}{parameters}{init}");
            if (ctorNode.Body != null)
            {
                sb.AppendLine("{");
                foreach (var stmt in ctorNode.Body.Statements) sb.AppendLine(stmt.NormalizeWhitespace().ToFullString());
                sb.AppendLine("}");
            }
            else if (ctorNode.ExpressionBody != null) sb.AppendLine($"=> {ctorNode.ExpressionBody.Expression.NormalizeWhitespace().ToFullString()};");
            else sb.AppendLine(";");
            return sb.ToString();
        }

        private static string FormatPropertySafe(PropertyDeclarationSyntax propNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in propNode.AttributeLists) sb.AppendLine(attr.NormalizeWhitespace().ToFullString());
            var modifiers = string.Join(" ", propNode.Modifiers.Select(m => m.Text));
            var type = propNode.Type.NormalizeWhitespace().ToFullString();
            sb.Append($"{modifiers} {type} {propNode.Identifier.Text}");
            if (propNode.AccessorList != null)
            {
                sb.AppendLine("\n{");
                foreach (var acc in propNode.AccessorList.Accessors) sb.AppendLine(acc.NormalizeWhitespace().ToFullString());
                sb.AppendLine("}");
            }
            else if (propNode.ExpressionBody != null) sb.AppendLine($" => {propNode.ExpressionBody.Expression.NormalizeWhitespace().ToFullString()};");
            else sb.AppendLine(";");

            if (propNode.Initializer != null) sb.AppendLine($" = {propNode.Initializer.Value.NormalizeWhitespace().ToFullString()};");
            return sb.ToString();
        }

        private static string FormatFieldSafe(FieldDeclarationSyntax fieldNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in fieldNode.AttributeLists) sb.AppendLine(attr.NormalizeWhitespace().ToFullString());
            var modifiers = string.Join(" ", fieldNode.Modifiers.Select(m => m.Text));
            var type = fieldNode.Declaration.Type.NormalizeWhitespace().ToFullString();
            var vars = string.Join(", ", fieldNode.Declaration.Variables.Select(v => v.NormalizeWhitespace().ToFullString()));
            sb.AppendLine($"{modifiers} {type} {vars};");
            return sb.ToString();
        }
    }
}
