using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    public class CliModuleParseTests
    {
        [Fact]
        public void ToPaths_ParsesSwitchCorrectly()
        {
            var code = @"
public class MyCli
{
    public void Run(string command)
    {
        switch (command)
        {
            case ""post"":
                int count = 5; // The count parameter
                string name = ""test""; //
                break;
            case ""put"":
                break;
            case ""delete"":
                break;
            case ""options"":
                break;
            case ""head"":
                break;
            case ""patch"":
                break;
            case ""trace"":
                break;
            case ""query"":
                break;
        }
    }
}
";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var classNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.CliModule.Parse.ToPaths(classNode);

            Assert.NotNull(paths);
            Assert.True(paths.ContainsKey("/post"));
            Assert.True(paths.ContainsKey("/put"));
            Assert.True(paths.ContainsKey("/delete"));
            Assert.True(paths.ContainsKey("/options"));
            Assert.True(paths.ContainsKey("/head"));
            Assert.True(paths.ContainsKey("/patch"));
            Assert.True(paths.ContainsKey("/trace"));
            Assert.True(paths.ContainsKey("/query"));

            var postPath = paths["/post"];
            Assert.NotNull(postPath.Post);
            Assert.NotNull(postPath.Parameters);
            Assert.Equal(2, postPath.Parameters.Count);

            var countParam = postPath.Parameters[0];
            Assert.Equal("count", countParam.Name);
            Assert.Equal("integer", countParam.Schema.Type);
            Assert.Equal(5, countParam.Example);
            Assert.Equal("The count parameter", countParam.Description);

            var nameParam = postPath.Parameters[1];
            Assert.Equal("name", nameParam.Name);
            Assert.Equal("string", nameParam.Schema.Type);
            Assert.Equal("test", nameParam.Example);
            Assert.Null(nameParam.Description);
        }

        [Fact]
        public void ToPaths_HandlesNonStringCasesAndNoInitializer()
        {
            var code = @"
public class MyCli
{
    public void Run(string command)
    {
        switch (command)
        {
            case ""post"":
                int no_init; // Parameter without initializer
                break;
            case 123:
                break;
            case nameof(MyCli):
                break;
        }
    }
}
";
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var classNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.CliModule.Parse.ToPaths(classNode);

            Assert.NotNull(paths);
            Assert.True(paths.ContainsKey("/post"));
            var postPath = paths["/post"];
            var param = postPath.Parameters[0];
            Assert.Equal("no-init", param.Name);
            Assert.Null(param.Example);
        }
    }
}
