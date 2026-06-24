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
            foreach (var usingDir in ns.Usings) sb.AppendLine(WasmSafeFormatter.Format(usingDir));
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
            return WasmSafeFormatter.Format(member);
        }

        private static string FormatClassSafe(ClassDeclarationSyntax classNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in classNode.AttributeLists) sb.AppendLine(WasmSafeFormatter.Format(attr));
            var modifiers = string.Join(" ", classNode.Modifiers.Select(m => m.Text));
            var baseList = classNode.BaseList != null ? $" {WasmSafeFormatter.Format(classNode.BaseList)}" : "";
            sb.AppendLine($"{modifiers} class {classNode.Identifier.Text}{baseList}\n{{");
            foreach (var member in classNode.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string FormatInterfaceSafe(InterfaceDeclarationSyntax interfaceNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in interfaceNode.AttributeLists) sb.AppendLine(WasmSafeFormatter.Format(attr));
            var modifiers = string.Join(" ", interfaceNode.Modifiers.Select(m => m.Text));
            var baseList = interfaceNode.BaseList != null ? $" {WasmSafeFormatter.Format(interfaceNode.BaseList)}" : "";
            sb.AppendLine($"{modifiers} interface {interfaceNode.Identifier.Text}{baseList}\n{{");
            foreach (var member in interfaceNode.Members) sb.AppendLine(FormatMemberSafe(member));
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string FormatMethodSafe(MethodDeclarationSyntax methodNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in methodNode.AttributeLists) sb.AppendLine(WasmSafeFormatter.Format(attr));
            var modifiers = string.Join(" ", methodNode.Modifiers.Select(m => m.Text));
            var returnType = WasmSafeFormatter.Format(methodNode.ReturnType);
            var parameters = WasmSafeFormatter.Format(methodNode.ParameterList);
            sb.AppendLine($"{modifiers} {returnType} {methodNode.Identifier.Text}{parameters}");
            if (methodNode.Body != null)
            {
                sb.AppendLine("{");
                foreach (var stmt in methodNode.Body.Statements) sb.AppendLine(WasmSafeFormatter.Format(stmt));
                sb.AppendLine("}");
            }
            else if (methodNode.ExpressionBody != null) sb.AppendLine($"=> {WasmSafeFormatter.Format(methodNode.ExpressionBody.Expression)};");
            else sb.AppendLine(";");
            return sb.ToString();
        }

        private static string FormatConstructorSafe(ConstructorDeclarationSyntax ctorNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in ctorNode.AttributeLists) sb.AppendLine(WasmSafeFormatter.Format(attr));
            var modifiers = string.Join(" ", ctorNode.Modifiers.Select(m => m.Text));
            var parameters = WasmSafeFormatter.Format(ctorNode.ParameterList);
            var init = ctorNode.Initializer != null ? $" {WasmSafeFormatter.Format(ctorNode.Initializer)}" : "";
            sb.AppendLine($"{modifiers} {ctorNode.Identifier.Text}{parameters}{init}");
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

        private static string FormatPropertySafe(PropertyDeclarationSyntax propNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in propNode.AttributeLists) sb.AppendLine(WasmSafeFormatter.Format(attr));
            var modifiers = string.Join(" ", propNode.Modifiers.Select(m => m.Text));
            var type = WasmSafeFormatter.Format(propNode.Type);
            sb.Append($"{modifiers} {type} {propNode.Identifier.Text}");
            if (propNode.AccessorList != null)
            {
                sb.AppendLine("\n{");
                foreach (var acc in propNode.AccessorList.Accessors) sb.AppendLine(WasmSafeFormatter.Format(acc));
                sb.AppendLine("}");
            }
            else if (propNode.ExpressionBody != null) sb.AppendLine($" => {WasmSafeFormatter.Format(propNode.ExpressionBody.Expression)};");
            else sb.AppendLine(";");

            if (propNode.Initializer != null) sb.AppendLine($" = {WasmSafeFormatter.Format(propNode.Initializer.Value)};");
            return sb.ToString();
        }

        private static string FormatFieldSafe(FieldDeclarationSyntax fieldNode)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var attr in fieldNode.AttributeLists) sb.AppendLine(WasmSafeFormatter.Format(attr));
            var modifiers = string.Join(" ", fieldNode.Modifiers.Select(m => m.Text));
            var type = WasmSafeFormatter.Format(fieldNode.Declaration.Type);
            var vars = string.Join(", ", fieldNode.Declaration.Variables.Select(v => WasmSafeFormatter.Format(v)));
            sb.AppendLine($"{modifiers} {type} {vars};");
            return sb.ToString();
        }
    }
}
