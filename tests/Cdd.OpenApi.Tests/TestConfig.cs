using Xunit;

namespace Cdd.OpenApi.Tests
{
    [CollectionDefinition("Cli", DisableParallelization = true)]
    public class CliCollectionDefinition
    {
    }

    public static class TestConfig
    {
        public static string TF = "net10.0";
    }
}
