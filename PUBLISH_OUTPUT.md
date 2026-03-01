# Publishing the Generated Client

When you use `cdd-csharp` to generate an API client library from an OpenAPI spec, you'll want to publish it to NuGet to share it with your organization or the public.

## Continuous Delivery via GitHub Actions

You can set up a GitHub Action cron job to periodically fetch the latest OpenAPI spec, generate the client, and publish it if there are changes:

```yaml
name: Generate and Publish Client

on:
  schedule:
    - cron: '0 0 * * *' # Run daily
  workflow_dispatch:

jobs:
  publish-client:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      - name: Install cdd-csharp
        run: make build
      - name: Download Spec and Generate
        run: |
          curl -s https://api.example.com/openapi.json -o spec.json
          ./bin/cdd-csharp from_openapi -i spec.json -o ./GeneratedClient
      - name: Check for Changes
        run: |
          if [ -n "$(git status --porcelain)" ]; then
            git config --global user.name "Bot"
            git config --global user.email "bot@example.com"
            git add .
            git commit -m "Update API Client"
            git push
            cd GeneratedClient
            dotnet pack -c Release
            dotnet nuget push bin/Release/*.nupkg -k ${{ secrets.NUGET_API_KEY }} -s https://api.nuget.org/v3/index.json
          else
            echo "No changes in API."
          fi
```
