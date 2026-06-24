using System.Collections.Generic;
using System.Text;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates unit tests for DAOs.</summary>
    public static class DaoTestsGenerator
    {
        /// <summary>Generates DAO tests.</summary>
        public static List<GeneratedCode> GenerateDaoTests(IDictionary<string, OpenApiSchema> schemas, string baseNamespace)
        {
            var results = new List<GeneratedCode>();

            var diTestCode = new StringBuilder();
            diTestCode.AppendLine($@"namespace {baseNamespace}.Tests
{{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using {baseNamespace}.Daos;
    using {baseNamespace}.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>Unit tests for DaoServiceCollectionExtensions.</summary>
    public class DaoServiceCollectionExtensionsTests
    {{
        /// <summary>Verifies DI routes to Stub DAOs by default.</summary>
        [Fact]
        public void AddDaos_Default_RegistersStubs()
        {{
            var services = new ServiceCollection();
            services.AddDaos(false, null);
            var provider = services.BuildServiceProvider();
");

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                diTestCode.AppendLine($@"            var {schemaName.ToLowerInvariant()}Dao = provider.GetRequiredService<I{schemaName}Dao>();");
                diTestCode.AppendLine($@"            Assert.IsType<Stub{schemaName}Dao>({schemaName.ToLowerInvariant()}Dao);");
            }

            diTestCode.AppendLine($@"        }}

        /// <summary>Verifies DI routes to Concrete DAOs with Ephemeral DB.</summary>
        [Fact]
        public void AddDaos_Ephemeral_RegistersConcrete()
        {{
            var services = new ServiceCollection();
            services.AddDaos(true, null);
            var provider = services.BuildServiceProvider();
");

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                diTestCode.AppendLine($@"            var {schemaName.ToLowerInvariant()}Dao = provider.GetRequiredService<I{schemaName}Dao>();");
                diTestCode.AppendLine($@"            Assert.IsType<Concrete{schemaName}Dao>({schemaName.ToLowerInvariant()}Dao);");
            }

            diTestCode.AppendLine($@"        }}

        /// <summary>Verifies DI routes to Concrete DAOs with Postgres.</summary>
        [Fact]
        public void AddDaos_Postgres_RegistersConcrete()
        {{
            var services = new ServiceCollection();
            services.AddDaos(false, ""Host=localhost;Database=test"");
            var provider = services.BuildServiceProvider();
");

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                diTestCode.AppendLine($@"            var {schemaName.ToLowerInvariant()}Dao = provider.GetRequiredService<I{schemaName}Dao>();");
                diTestCode.AppendLine($@"            Assert.IsType<Concrete{schemaName}Dao>({schemaName.ToLowerInvariant()}Dao);");
            }

            diTestCode.AppendLine($@"        }}
    }}
}}");
            results.Add(new GeneratedCode { FileName = "src/Tests/DaoServiceCollectionExtensionsTests.cs", Code = diTestCode.ToString() });

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                var schema = schemaKvp.Value;

                bool hasId = false;
                if (schema.Properties != null)
                {
                    foreach (var prop in schema.Properties)
                    {
                        var propNameLower = prop.Key.ToLowerInvariant();
                        if (propNameLower == "id") hasId = true;
                    }
                }

                if (!hasId) continue;

                var schemaTestCode = new StringBuilder();
                schemaTestCode.AppendLine($@"namespace {baseNamespace}.Tests
{{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using {baseNamespace}.Daos;
    using {baseNamespace}.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>Unit tests for {schemaName}Dao.</summary>
    public class {schemaName}DaoTests
    {{
        /// <summary>Tests basic CRUD operations on Concrete{schemaName}Dao using Sqlite In-Memory.</summary>
        [Fact]
        public async Task Crud_Works()
        {{
            var services = new ServiceCollection();
            services.AddDaos(true, null);
            var provider = services.BuildServiceProvider();

            // Setup DB
            var dbContext = provider.GetRequiredService<AppDbContext>();
            await dbContext.Database.OpenConnectionAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var dao = provider.GetRequiredService<I{schemaName}Dao>();

            // We must mock any properties that might violate NOT NULL constraints in SQLite for tests
            // Create
            var faker = new Bogus.Faker<{schemaName}>()
                .RuleForType(typeof(string), f => f.Lorem.Word())
                .RuleForType(typeof(int), f => f.Random.Int(1, 100))
                .RuleForType(typeof(long), f => f.Random.Long(1, 100))
                .RuleForType(typeof(bool), f => f.Random.Bool());
            var newEntity = faker.Generate();

            // Populate required objects
            if (newEntity != null)
            {{
                await dao.CreateAsync(newEntity);
            }}

            // Read
            var all = await dao.GetAllAsync();
            Assert.NotEmpty(all);

            // Ensure DB Cleanup
            await dbContext.Database.EnsureDeletedAsync();
        }}
    }}
}}");
                results.Add(new GeneratedCode { FileName = $"src/Tests/{schemaName}DaoTests.cs", Code = schemaTestCode.ToString() });
            }

            return results;
        }
    }
}
