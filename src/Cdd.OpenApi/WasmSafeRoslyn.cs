using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Cdd.OpenApi
{
    public static class WasmSafeRoslyn
    {
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
                var children = current.ChildNodesAndTokens();
                foreach (var childOrToken in children.Reverse())
                {
                    if (childOrToken.IsNode)
                    {
                        stack.Push(childOrToken.AsNode()!);
                    }
                }
            }
        }


    }
}
