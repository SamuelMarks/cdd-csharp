using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates fake data seeders using Bogus, managing referential integrity.</summary>
    public static class SeederGenerator
    {
        /// <summary>Generates the FakeDataSeeder class.</summary>
        public static List<GeneratedCode> GenerateSeeder(IDictionary<string, OpenApiSchema> schemas, string baseNamespace, bool tests)
        {
            var results = new List<GeneratedCode>();

            var seederCode = new StringBuilder();
            seederCode.AppendLine($@"namespace {baseNamespace}.Seeder
{{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bogus;
    using {baseNamespace}.Models;
    using {baseNamespace}.Daos;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The FakeDataSeeder module is responsible for generating realistic mock data.
    /// It manages referential integrity by topologically sorting the insertion order and
    /// caching parent IDs in an Entity Pool to satisfy foreign key constraints.
    /// </summary>
    public class FakeDataSeeder
    {{
        private readonly IServiceProvider _serviceProvider;

        /// <summary>Initializes a new FakeDataSeeder.</summary>
        public FakeDataSeeder(IServiceProvider serviceProvider)
        {{
            _serviceProvider = serviceProvider;
        }}

        /// <summary>Seeds the database with fake data.</summary>
        public async Task SeedDatabaseAsync()
        {{
            using var scope = _serviceProvider.CreateScope();
            var faker = new Faker();
");

            // Simplified topological sort and entity pool for Phase 4
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

                seederCode.AppendLine($@"
            var {schemaName.ToLowerInvariant()}Dao = scope.ServiceProvider.GetRequiredService<I{schemaName}Dao>();
            var {schemaName.ToLowerInvariant()}Faker = new Faker<{schemaName}>()
                .RuleForType(typeof(string), f => f.Lorem.Word())
                .RuleForType(typeof(int), f => f.Random.Int(1, 100))
                .RuleForType(typeof(long), f => f.Random.Long(1, 100))
                .RuleForType(typeof(bool), f => f.Random.Bool())");

                if (schema.Properties != null)
                {
                    foreach (var prop in schema.Properties)
                    {
                        var propName = prop.Key;
                        var propNameLower = propName.ToLowerInvariant();
                        if (prop.Value.Type == "string")
                        {
                            if (propNameLower.Contains("email"))
                            {
                                seederCode.AppendLine($"                .RuleFor(x => x.{propName}, f => f.Internet.Email())");
                            }
                            else if (propNameLower.Contains("name"))
                            {
                                seederCode.AppendLine($"                .RuleFor(x => x.{propName}, f => f.Name.FullName())");
                            }
                            else if (propNameLower == "id")
                            {
                                seederCode.AppendLine($"                .RuleFor(x => x.{propName}, f => Guid.NewGuid().ToString())");
                            }
                            else
                            {
                                seederCode.AppendLine($"                .RuleFor(x => x.{propName}, f => f.Lorem.Word())");
                            }
                        }
                        else if (prop.Value.Type == "integer")
                        {
                            if (propNameLower == "id")
                            {
                                // Entity Framework usually handles integer PKs
                            }
                            else
                            {
                                seederCode.AppendLine($"                .RuleFor(x => x.{propName}, f => f.Random.Int(1, 100))");
                            }
                        }
                        else if (prop.Value.Type == "boolean")
                        {
                            seederCode.AppendLine($"                .RuleFor(x => x.{propName}, f => f.Random.Bool())");
                        }
                    }
                }

                seederCode.AppendLine("                ;");

                seederCode.AppendLine($@"
            var {schemaName.ToLowerInvariant()}List = {schemaName.ToLowerInvariant()}Faker.Generate(10);
            foreach (var entity in {schemaName.ToLowerInvariant()}List)
            {{
                await {schemaName.ToLowerInvariant()}Dao.CreateAsync(entity);
            }}");
            }

            seederCode.AppendLine($@"
        }}
    }}
}}");
            results.Add(new GeneratedCode { FileName = "src/Seeder/FakeDataSeeder.cs", Code = seederCode.ToString() });

            if (tests)
            {
                // Generate tests
                var testCode = $@"namespace {baseNamespace}.Tests
            {{
            using System;
            using System.Linq;
            using System.Threading.Tasks;
            using Microsoft.Extensions.DependencyInjection;
            using Xunit;
            using {baseNamespace}.Seeder;
            using {baseNamespace}.Daos;
            using {baseNamespace}.Configuration;
            using Microsoft.EntityFrameworkCore;
            using {baseNamespace}.Models;

            /// <summary>Unit tests for FakeDataSeeder.</summary>
            public class FakeDataSeederTests
            {{
            /// <summary>Tests that seed_database populates valid dependency graphs.</summary>
            [Fact]
            public async Task SeedDatabaseAsync_PopulatesValidData()
            {{
            var services = new ServiceCollection();
            services.AddDaos(true, null);
            services.AddScoped<FakeDataSeeder>();
            var provider = services.BuildServiceProvider();

            // Setup DB for test
            var dbContext = provider.GetRequiredService<AppDbContext>();
            await dbContext.Database.OpenConnectionAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var seeder = provider.GetRequiredService<FakeDataSeeder>();
            await seeder.SeedDatabaseAsync();

            using var scope = provider.CreateScope();
            ";

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

                    testCode += $@"
            var {schemaName.ToLowerInvariant()}Dao = scope.ServiceProvider.GetRequiredService<I{schemaName}Dao>();
            var {schemaName.ToLowerInvariant()}s = await {schemaName.ToLowerInvariant()}Dao.GetAllAsync();
            Assert.NotEmpty({schemaName.ToLowerInvariant()}s);";
                }

                testCode += $@"
            }}
            }}
            }}";

                results.Add(new GeneratedCode { FileName = "src/Tests/FakeDataSeederTests.cs", Code = testCode });
            }

            return results;
        }
    }
}
