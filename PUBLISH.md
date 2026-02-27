# Publishing CDD C# OpenAPI Tools

This document outlines how to publish the `Cdd.OpenApi` library and `Cdd.OpenApi.Cli` to the most popular .NET package registry (**NuGet**), and how to generate and host its documentation.

## 1. Publishing to NuGet (nuget.org)

NuGet is the official package manager for .NET. 

### Prerequisites
1. Create an account on [nuget.org](https://www.nuget.org/).
2. Go to **API Keys** and generate a new key with `Push` permissions.
3. Keep the key secure.

### Packaging and Pushing

Update the `<Version>` tag in your `.csproj` files, then run the following commands:

```bash
# Build and package the project in Release mode
dotnet pack src/Cdd.OpenApi/Cdd.OpenApi.csproj -c Release
dotnet pack src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release

# Push the library to NuGet
dotnet nuget push src/Cdd.OpenApi/bin/Release/*.nupkg 
  --api-key YOUR_NUGET_API_KEY 
  --source https://api.nuget.org/v3/index.json

# Push the CLI tool to NuGet
dotnet nuget push src/Cdd.OpenApi.Cli/bin/Release/*.nupkg 
  --api-key YOUR_NUGET_API_KEY 
  --source https://api.nuget.org/v3/index.json
```

*(Note: For the CLI to be consumable as a global `dotnet tool`, ensure `<PackAsTool>true</PackAsTool>` is set in `Cdd.OpenApi.Cli.csproj`.)*

---

## 2. Generating and Hosting Documentation Locally

The standard for C# API documentation is **DocFX**. It reads your `/// <summary>` XML docstrings and generates a static website.

### Setup and Build

```bash
# Install DocFX globally
dotnet tool install -g docfx

# Initialize a new docfx project (creates docfx.json)
docfx init -q

# Build the documentation (outputs to the _site folder)
docfx build docfx.json
```

### Serve Locally

DocFX includes a built-in static file server:

```bash
# Serves the _site directory on http://localhost:8080
docfx serve _site
```
Alternatively, using Python: `python3 -m http.server --directory _site 8080`.

---

## 3. Publishing Documentation to the Web (GitHub Pages)

**GitHub Pages** is the most popular and free place to host open-source documentation.

You can automate DocFX generation and deployment using GitHub Actions. Create a file at `.github/workflows/docs.yml`:

```yaml
name: Publish Documentation

on:
  push:
    branches: [ "main" ]

permissions:
  contents: write

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Install DocFX
        run: dotnet tool install -g docfx

      - name: Build Docs
        run: docfx build docfx.json

      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./_site
```

Whenever you push to `main`, this action will build your static documentation folder (`_site`) and deploy it to the `gh-pages` branch, making it live at `https://<your-username>.github.io/cdd-csharp/`.
