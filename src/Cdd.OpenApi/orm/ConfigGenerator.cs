using System.Collections.Generic;
using System.Text;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates server configuration and database initialization routines.</summary>
    public static class ConfigGenerator
    {
        /// <summary>Generates the ServerConfiguration and AppInitializer classes.</summary>
        public static List<GeneratedCode> GenerateConfig(string baseNamespace)
        {
            var results = new List<GeneratedCode>();

            var configCode = $@"namespace {baseNamespace}.Configuration
{{
    using System;

    /// <summary>Represents the server configuration derived from environment variables and CLI flags.</summary>
    public class ServerConfiguration
    {{
        /// <summary>Gets or sets a value indicating whether to use an ephemeral, in-memory database.</summary>
        public bool UseEphemeral {{ get; set; }}

        /// <summary>Gets or sets the connection string for the primary database. Overridden if UseEphemeral is true.</summary>
        public string? DatabaseUrl {{ get; set; }}

        /// <summary>Gets or sets a value indicating whether to run the fake data seeder on startup.</summary>
        public bool Seed {{ get; set; }}

        /// <summary>Creates a configuration from environment variables.</summary>
        public static ServerConfiguration FromEnvironment()
        {{
            var config = new ServerConfiguration();

            var dbUrl = Environment.GetEnvironmentVariable(""DATABASE_URL"");
            if (!string.IsNullOrEmpty(dbUrl))
            {{
                config.DatabaseUrl = dbUrl;
            }}

            var ephemeral = Environment.GetEnvironmentVariable(""EPHEMERAL_DB"");
            if (ephemeral == ""true"" || ephemeral == ""1"")
            {{
                config.UseEphemeral = true;
            }}

            return config;
        }}
    }}
}}";
            results.Add(new GeneratedCode { FileName = "src/Configuration/ServerConfiguration.cs", Code = configCode });

            var initializerCode = $@"namespace {baseNamespace}.Configuration
{{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using {baseNamespace}.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>Handles application startup tasks such as database migrations and seeding.</summary>
    public static class AppInitializer
    {{
        /// <summary>Initializes the database programmatically.</summary>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {{
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<AppDbContext>();

            if (context != null)
            {{
                // Open connection if sqlite in-memory
                if (context.Database.IsSqlite())
                {{
                    await context.Database.OpenConnectionAsync();
                    await context.Database.EnsureCreatedAsync();
                }}
                else
                {{
                    // Use migrations for Postgres, or EnsureCreated for simplicity in stubs
                    await context.Database.EnsureCreatedAsync();
                }}
            }}
        }}
    }}
}}";
            results.Add(new GeneratedCode { FileName = "src/Configuration/AppInitializer.cs", Code = initializerCode });

            return results;
        }
    }
}
