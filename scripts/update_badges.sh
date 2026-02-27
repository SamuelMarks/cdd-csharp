#!/bin/bash
set -e

echo "Running tests to compute coverage..."

# Check test coverage
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
dotnet build CddOpenApi.slnx -q
~/.dotnet/tools/coverlet tests/Cdd.OpenApi.Tests/bin/Debug/net10.0/Cdd.OpenApi.Tests.dll --target "dotnet" --targetargs "test tests/Cdd.OpenApi.Tests --no-build" --format opencover --output coverage.xml > /dev/null

LINE_COV=$(grep -oP 'sequenceCoverage="\K[^"]+' coverage.xml | head -1)

# Ensure it's not empty
if [ -z "$LINE_COV" ]; then LINE_COV="0"; fi

# Calculate actual Doc Coverage using the Roslyn analyzer script
dotnet build scripts/DocCoverage.csproj -q
DOC_COV=$(dotnet run --project scripts/DocCoverage.csproj src/Cdd.OpenApi)

echo "Test Coverage: $LINE_COV%"
echo "Doc Coverage: $DOC_COV%"

# Color logic
TEST_COLOR="brightgreen"
if [ "$LINE_COV" != "100" ]; then TEST_COLOR="yellow"; fi
if [ "$LINE_COV" != "100" ] && [ "${LINE_COV%%.*}" -lt 80 ]; then TEST_COLOR="red"; fi

DOC_COLOR="brightgreen"
if [ "$DOC_COV" -lt "100" ]; then DOC_COLOR="yellow"; fi
if [ "$DOC_COV" -lt "80" ]; then DOC_COLOR="red"; fi

# Update README.md shields
sed -i -E "s/badge\/Coverage-[0-9.]+[^%-]*%25-[a-z]+/badge\/Coverage-$LINE_COV%25-$TEST_COLOR/" README.md
sed -i -E "s/badge\/Docs-[0-9.]+[^%-]*%25-[a-z]+/badge\/Docs-$DOC_COV%25-$DOC_COLOR/" README.md

echo "README.md updated with latest badges."
