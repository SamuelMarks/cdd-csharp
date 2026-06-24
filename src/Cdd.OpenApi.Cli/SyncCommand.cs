using System;
using System.IO;
using System.Linq;

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Implements the sync command which synchronizes source code and OpenAPI specifications.
    /// </summary>
    public static class SyncCommand
    {
        /// <summary>
        /// Runs the sync command with the specified arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The exit code.</returns>
        public static int Run(string[] args)
        {
            string truth = string.Empty;
            string inputPath = string.Empty;
            string outputPath = string.Empty;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--truth" && i + 1 < args.Length)
                {
                    truth = args[++i];
                }
                else if ((args[i] == "-i" || args[i] == "--input") && i + 1 < args.Length)
                {
                    inputPath = args[++i];
                }
                else if ((args[i] == "-o" || args[i] == "--output") && i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
            }

            if (string.IsNullOrEmpty(truth))
            {
                Console.Error.WriteLine("Usage: cdd-csharp sync --truth <truth> -i <input> [-o <output>]\n\nOptions:\n  --truth        The source of truth (e.g., class, sqlalchemy, function).\n  -i, --input    Path to the source files.\n  -o, --output   Output file or directory path.");
                return 1;
            }

            // A placeholder implementation for the sync command which just calls to_openapi if truth is class
            // In a real implementation this would parse the truth source and update the others.
            if (truth == "class" || truth == "function")
            {
                Console.WriteLine($"Synchronizing from truth '{truth}'...");
                return CddCli.GenerateToOpenApi(args);
            }
            else
            {
                Console.Error.WriteLine($"Error: Truth source '{truth}' is not supported yet.");
                return 1;
            }
        }
    }
}
