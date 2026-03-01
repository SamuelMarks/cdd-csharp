# Publishing cdd-csharp

## Publishing to NuGet

To publish the library and the CLI to NuGet (`nuget.org`):

1. **Build and Pack**:
   ```bash
   dotnet pack src/Cdd.OpenApi/Cdd.OpenApi.csproj -c Release -o ./artifacts
   dotnet pack src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release -o ./artifacts
   ```

2. **Push to NuGet**:
   ```bash
   dotnet nuget push ./artifacts/*.nupkg -k YOUR_NUGET_API_KEY -s https://api.nuget.org/v3/index.json
   ```

## Publishing Documentation

You can generate the documentation locally:
```bash
make build_docs
```

This places XML documentation in the `docs/` directory, which can be served statically using tools like DocFX or standard web servers.
