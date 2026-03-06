using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.CliModule
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for ToCli.</summary>
        public static ClassDeclarationSyntax ToCli(string className, OpenApiPaths paths)
        {
            var classNode = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var mainMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("int"), "Main")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")).WithType(SyntaxFactory.ParseTypeName("string[]")));

            var bodyStatements = new List<StatementSyntax>();

            bodyStatements.Add(SyntaxFactory.ParseStatement("string globalServer = null;"));
            bodyStatements.Add(SyntaxFactory.ParseStatement("int cmdIndex = 0;"));
            bodyStatements.Add(SyntaxFactory.ParseStatement("if (args.Length > 1 && args[0] == \"--server\") { globalServer = args[1]; cmdIndex = 2; }"));

            var helpCheck = SyntaxFactory.ParseStatement("if (args.Length <= cmdIndex || args[cmdIndex] == \"--help\") { System.Console.WriteLine(\"Usage: \" + nameof(" + className + ") + \" [--server <url>] <command> [options]\"); return 0; }");
            bodyStatements.Add(helpCheck);

            var switchStatement = SyntaxFactory.SwitchStatement(SyntaxFactory.ParseExpression("args[cmdIndex]"));

            foreach (var pathKvp in paths)
            {
                var routePath = pathKvp.Key;
                var pathItem = pathKvp.Value;
                
                var ops = new Dictionary<string, OpenApiOperation?>
                {
                    { "get", pathItem.Get },
                    { "post", pathItem.Post },
                    { "put", pathItem.Put },
                    { "delete", pathItem.Delete },
                    { "options", pathItem.Options },
                    { "head", pathItem.Head },
                    { "patch", pathItem.Patch },
                    { "trace", pathItem.Trace },
                    { "query", pathItem.Query }
                };

                foreach (var opKvp in ops)
                {
                    var op = opKvp.Value;
                    if (op == null) continue;
                    var operationId = op.OperationId ?? (opKvp.Key + routePath.Replace("/", "").Replace("{", "").Replace("}", ""));
                    var commandName = operationId.ToLower();
                    
                    var description = op.Summary ?? "No description";
                    
                    var caseLabel = SyntaxFactory.CaseSwitchLabel(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(commandName)));
                    
                    var caseStatements = new SyntaxList<StatementSyntax>();

                    string helpStr = $"System.Console.WriteLine(\"{description}\");";
                    
                    if (op.Parameters != null && op.Parameters.Any())
                    {
                        helpStr += " System.Console.WriteLine(\"Options:\");";
                        foreach (var p in op.Parameters)
                        {
                            string exampleStr = "";
                            if (p.Example != null) exampleStr = $" (Example: {p.Example})";
                            else if (p.Examples != null && p.Examples.Any()) exampleStr = $" (Example: {p.Examples.First().Value.Value})";
                            
                            helpStr += $" System.Console.WriteLine(\"  --{p.Name.ToLower()} <{p.Schema?.Type ?? "string"}> : {p.Description ?? "No description"}{exampleStr}\");";
                            
                            string varType = p.Schema?.Type == "integer" ? "int" : "string";
                            string varName = p.Name.Replace("-", "_");
                            caseStatements = caseStatements.Add(SyntaxFactory.ParseStatement($"{varType} {varName} = default;"));
                        }
                        
                        var loopCode = "for (int i = cmdIndex + 1; i < args.Length; i++) { ";
                        foreach (var p in op.Parameters)
                        {
                            string varType = p.Schema?.Type == "integer" ? "int" : "string";
                            string varName = p.Name.Replace("-", "_");
                            string flagName = $"--{p.Name.ToLower()}";
                            if (varType == "int") {
                                loopCode += $"if (args[i] == \"{flagName}\" && i + 1 < args.Length) {{ if (int.TryParse(args[++i], out var temp_{varName})) {varName} = temp_{varName}; }} ";
                            } else {
                                loopCode += $"if (args[i] == \"{flagName}\" && i + 1 < args.Length) {{ {varName} = args[++i]; }} ";
                            }
                        }
                        loopCode += "}";
                        caseStatements = caseStatements.Add(SyntaxFactory.ParseStatement(loopCode));
                        
                        string executedArgs = string.Join(", ", op.Parameters.Select(p => $"{p.Name.Replace("-", "_")}={{{p.Name.Replace("-", "_")}}}"));
                        caseStatements = caseStatements.Add(SyntaxFactory.ParseStatement($"System.Console.WriteLine($\"Executing {commandName} with {executedArgs}...\");"));
                    }
                    else
                    {
                        caseStatements = caseStatements.Add(SyntaxFactory.ParseStatement($"System.Console.WriteLine(\"Executing {commandName}...\");"));
                    }

                    var cmdHelpCheck = SyntaxFactory.ParseStatement($"if (args.Length > cmdIndex + 1 && args[cmdIndex + 1] == \"--help\") {{ {helpStr} return 0; }}");
                    caseStatements = caseStatements.Insert(0, cmdHelpCheck);
                    
                    caseStatements = caseStatements.Add(SyntaxFactory.BreakStatement());

                    switchStatement = switchStatement.AddSections(SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>().Add(caseLabel), caseStatements));
                }
            }

            var defaultLabel = SyntaxFactory.DefaultSwitchLabel();
            var defaultStatements = new SyntaxList<StatementSyntax>()
                .Add(SyntaxFactory.ParseStatement("System.Console.WriteLine(\"Unknown command\");"))
                .Add(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))));

            switchStatement = switchStatement.AddSections(SyntaxFactory.SwitchSection(new SyntaxList<SwitchLabelSyntax>().Add(defaultLabel), defaultStatements));
            bodyStatements.Add(switchStatement);
            bodyStatements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))));

            mainMethod = mainMethod.WithBody(SyntaxFactory.Block(bodyStatements));
            classNode = classNode.AddMembers(mainMethod);

            return classNode.NormalizeWhitespace();
        }
    }
}