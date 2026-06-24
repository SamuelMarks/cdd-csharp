using System.Collections.Generic;
using System.Text;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates Data Access Objects (DAOs) for the OpenAPI models.</summary>
    public static class DaoGenerator
    {
        /// <summary>Generates DAOs and their dependency injection extensions.</summary>
        public static List<GeneratedCode> GenerateDaos(IDictionary<string, OpenApiSchema> schemas, string baseNamespace)
        {
            var results = new List<GeneratedCode>();

            var diCode = new StringBuilder();
            diCode.AppendLine($@"namespace {baseNamespace}.Daos
{{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using {baseNamespace}.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>Dependency injection extensions for DAOs.</summary>
    public static class DaoServiceCollectionExtensions
    {{
        /// <summary>Registers DAOs based on the environment configuration.</summary>
        public static IServiceCollection AddDaos(this IServiceCollection services, bool useEphemeral, string? databaseUrl)
        {{
            if (useEphemeral)
            {{
                var dbName = Guid.NewGuid().ToString();
                services.AddDbContext<AppDbContext>(options => options.UseSqlite($""DataSource={{dbName}};Mode=Memory;Cache=Shared""));
                // Register Concrete DAOs
");

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                diCode.AppendLine($"                services.AddScoped<I{schemaName}Dao, Concrete{schemaName}Dao>();");
            }

            diCode.AppendLine($@"            }}
            else if (!string.IsNullOrEmpty(databaseUrl))
            {{
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(databaseUrl));
                // Register Concrete DAOs
");
            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                diCode.AppendLine($"                services.AddScoped<I{schemaName}Dao, Concrete{schemaName}Dao>();");
            }

            diCode.AppendLine($@"            }}
            else
            {{
                // Register Stub DAOs
");
            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                diCode.AppendLine($"                services.AddScoped<I{schemaName}Dao, Stub{schemaName}Dao>();");
            }

            diCode.AppendLine($@"            }}
            return services;
        }}
    }}
}}");

            results.Add(new GeneratedCode { FileName = "src/Daos/DaoServiceCollectionExtensions.cs", Code = diCode.ToString() });

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                var schema = schemaKvp.Value;

                string idType = "string";
                if (schema.Properties != null && schema.Properties.TryGetValue("id", out var idProp))
                {
                    if (idProp.Type == "integer") idType = "int";
                }
                else if (schema.Properties != null && schema.Properties.TryGetValue("Id", out var upperIdProp))
                {
                    if (upperIdProp.Type == "integer") idType = "int";
                }

                var daoCode = $@"namespace {baseNamespace}.Daos
{{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using {baseNamespace}.Models;

    /// <summary>Abstract DAO interface for {schemaName}.</summary>
    public interface I{schemaName}Dao
    {{
        /// <summary>Gets all {schemaName} entities.</summary>
        Task<IEnumerable<{schemaName}>> GetAllAsync();
        /// <summary>Gets a single {schemaName} by ID.</summary>
        Task<{schemaName}?> GetByIdAsync({idType} id);
        /// <summary>Creates a new {schemaName}.</summary>
        Task<{schemaName}> CreateAsync({schemaName} entity);
        /// <summary>Updates an existing {schemaName}.</summary>
        Task<{schemaName}> UpdateAsync({schemaName} entity);
        /// <summary>Deletes a {schemaName} by ID.</summary>
        Task DeleteAsync({idType} id);
    }}

    /// <summary>Stub DAO implementation for {schemaName}.</summary>
    public class Stub{schemaName}Dao : I{schemaName}Dao
    {{
        /// <summary>Gets all {schemaName} entities.</summary>
        public Task<IEnumerable<{schemaName}>> GetAllAsync() => throw new NotImplementedException();
        /// <summary>Gets a single {schemaName} by ID.</summary>
        public Task<{schemaName}?> GetByIdAsync({idType} id) => throw new NotImplementedException();
        /// <summary>Creates a new {schemaName}.</summary>
        public Task<{schemaName}> CreateAsync({schemaName} entity) => throw new NotImplementedException();
        /// <summary>Updates an existing {schemaName}.</summary>
        public Task<{schemaName}> UpdateAsync({schemaName} entity) => throw new NotImplementedException();
        /// <summary>Deletes a {schemaName} by ID.</summary>
        public Task DeleteAsync({idType} id) => throw new NotImplementedException();
    }}

    /// <summary>Concrete DAO implementation for {schemaName} using Entity Framework Core.</summary>
    public class Concrete{schemaName}Dao : I{schemaName}Dao
    {{
        private readonly AppDbContext _context;

        /// <summary>Initializes a new instance of Concrete{schemaName}Dao.</summary>
        public Concrete{schemaName}Dao(AppDbContext context)
        {{
            _context = context;
        }}

        /// <summary>Gets all {schemaName} entities.</summary>
        public async Task<IEnumerable<{schemaName}>> GetAllAsync()
        {{
            return await _context.{schemaName}s.ToListAsync();
        }}

        /// <summary>Gets a single {schemaName} by ID.</summary>
        public async Task<{schemaName}?> GetByIdAsync({idType} id)
        {{
            return await _context.{schemaName}s.FindAsync(id);
        }}

        /// <summary>Creates a new {schemaName}.</summary>
        public async Task<{schemaName}> CreateAsync({schemaName} entity)
        {{
            _context.{schemaName}s.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }}

        /// <summary>Updates an existing {schemaName}.</summary>
        public async Task<{schemaName}> UpdateAsync({schemaName} entity)
        {{
            _context.{schemaName}s.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }}

        /// <summary>Deletes a {schemaName} by ID.</summary>
        public async Task DeleteAsync({idType} id)
        {{
            var entity = await _context.{schemaName}s.FindAsync(id);
            if (entity != null)
            {{
                _context.{schemaName}s.Remove(entity);
                await _context.SaveChangesAsync();
            }}
        }}
    }}
}}";
                results.Add(new GeneratedCode { FileName = $"src/Daos/{schemaName}Dao.cs", Code = daoCode });
            }

            return results;
        }
    }
}
