using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocCoverage {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) return;
            int totalPublic = 0;
            int documented = 0;
            
            foreach (var file in Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories)) {
                if (file.Contains("/obj/") || file.Contains("/bin/")) continue;
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                var root = tree.GetRoot();
                var nodes = root.DescendantNodes().Where(n => 
                    n is ClassDeclarationSyntax || 
                    n is MethodDeclarationSyntax || 
                    n is PropertyDeclarationSyntax || 
                    n is InterfaceDeclarationSyntax || 
                    n is EnumDeclarationSyntax || 
                    n is RecordDeclarationSyntax ||
                    n is DelegateDeclarationSyntax ||
                    n is ConstructorDeclarationSyntax);
                    
                foreach (var node in nodes) {
                    var isPublic = false;
                    if (node is BaseTypeDeclarationSyntax btd && btd.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) isPublic = true;
                    if (node is MethodDeclarationSyntax md && md.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) isPublic = true;
                    if (node is PropertyDeclarationSyntax pd && pd.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) isPublic = true;
                    if (node is DelegateDeclarationSyntax dd && dd.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) isPublic = true;
                    if (node is ConstructorDeclarationSyntax cd && cd.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) isPublic = true;
                    
                    if (isPublic) {
                        totalPublic++;
                        var hasDocs = node.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
                        if (hasDocs) documented++;
                    }
                }
            }
            
            if (totalPublic == 0) Console.WriteLine("100");
            else Console.WriteLine((int)((double)documented / totalPublic * 100));
        }
    }
}
