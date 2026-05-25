using Xunit;
using Cdd.OpenApi;

namespace Cdd.OpenApi.Tests
{
    public class EscapeTests
    {
        [Fact]
        public void GenerateAll_NoEscapeRequired()
        {
            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            System.IO.Directory.CreateDirectory(tempDir);
            try
            {
                var inputPath = System.IO.Path.Combine(tempDir, "spec.json");
                System.IO.File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""info"": { ""title"": ""Title"", ""description"": ""Desc"", ""version"": ""1.0"", ""contact"": { ""name"": ""Name"" } }, ""paths"": {} }");

                var config = new CddConfig { InputPath = inputPath, OutputDir = tempDir, NoGithubActions = true, NoInstallablePackage = false };
                CddGenerator.GenerateAll(config);
            }
            finally
            {
                System.IO.Directory.Delete(tempDir, true);
            }
        }
    }
}
