# Publishing cdd-csharp

## Publishing the C# Tool to NuGet
To publish the `cdd-csharp` CLI tool as a global tool on NuGet:

1. Ensure your `.csproj` file for the CLI has `PackAsTool` set to true.
2. Build the package:
   ```bash
   dotnet pack src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release
   ```
3. Publish to NuGet.org:
   ```bash
   dotnet nuget push src/Cdd.OpenApi.Cli/bin/Release/*.nupkg -k YOUR_NUGET_API_KEY -s https://api.nuget.org/v3/index.json
   ```

## Publishing Documentation
1. **Local Serving**:
   Build docs using DocFX (via `make build_docs`) and serve locally:
   ```bash
   dotnet tool install -g docfx
   docfx serve docs
   ```
2. **Publishing to GitHub Pages**:
   You can use a GitHub Action to deploy the `docs` directory output from DocFX to GitHub Pages.
