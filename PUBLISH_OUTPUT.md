# Publishing Output SDKs

When generating C# SDKs from an OpenAPI specification, you may want to automatically update your SDK package when the server specification changes.

## Automated Updates via GitHub Actions
Create a `.github/workflows/update-sdk.yml` file in your client SDK repository:

```yaml
name: Update API SDK

on:
  schedule:
    - cron: '0 0 * * *' # Run daily
  workflow_dispatch:

jobs:
  update-sdk:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Install cdd-csharp
        run: dotnet tool install -g cdd-csharp

      - name: Download latest OpenAPI Spec
        run: curl -sL "https://api.yourdomain.com/openapi.json" -o openapi.json

      - name: Generate SDK
        run: cdd-csharp from_openapi to_sdk -i openapi.json -o ./src

      - name: Check for changes
        id: git-check
        run: |
          git diff --exit-code || echo "changes=true" >> $GITHUB_OUTPUT

      - name: Commit and Create PR
        if: steps.git-check.outputs.changes == 'true'
        uses: peter-evans/create-pull-request@v5
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          commit-message: "chore: update SDK to match latest OpenAPI spec"
          title: "Automated SDK Update"
          branch: "update-sdk"

      - name: Build & Publish Package
        if: steps.git-check.outputs.changes == 'true'
        run: |
          dotnet pack src/YourClient.csproj -c Release
          dotnet nuget push src/bin/Release/*.nupkg -k ${{ secrets.NUGET_API_KEY }} -s https://api.nuget.org/v3/index.json
```
