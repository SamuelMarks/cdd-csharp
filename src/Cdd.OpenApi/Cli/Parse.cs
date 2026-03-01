using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.CliModule
{
    /// <summary>Auto-generated documentation for Parse.</summary>
    public static class Parse
    {
        /// <summary>Auto-generated documentation for ToPaths.</summary>
        public static OpenApiPaths ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new OpenApiPaths();
            
            var switchStmts = classNode.DescendantNodes().OfType<SwitchStatementSyntax>();
            foreach (var switchStmt in switchStmts)
            {
                foreach (var section in switchStmt.Sections)
                {
                    foreach (var label in section.Labels.OfType<CaseSwitchLabelSyntax>())
                    {
                        if (label.Value is LiteralExpressionSyntax literal && literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                        {
                            var commandName = literal.Token.ValueText;
                            
                            var pathItem = new OpenApiPathItem
                            {
                                Get = new OpenApiOperation
                                {
                                    OperationId = commandName,
                                    Summary = "Command " + commandName
                                }
                            };
                            
                            paths["/" + commandName] = pathItem;
                        }
                    }
                }
            }
            
            return paths;
        }
    }
}
