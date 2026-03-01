@echo off
setlocal

set COMMAND=%1
if "%COMMAND%"=="" set COMMAND=help

if "%COMMAND%"=="help" goto help
if "%COMMAND%"=="all" goto help
if "%COMMAND%"=="install_base" goto install_base
if "%COMMAND%"=="install_deps" goto install_deps
if "%COMMAND%"=="build_docs" goto build_docs
if "%COMMAND%"=="build" goto build
if "%COMMAND%"=="test" goto test
if "%COMMAND%"=="run" goto run
if "%COMMAND%"=="build_wasm" goto build_wasm
if "%COMMAND%"=="build_docker" goto build_docker
if "%COMMAND%"=="run_docker" goto run_docker

:help
echo Available tasks:
echo   install_base  - Install .NET runtime
echo   install_deps  - Restore dependencies
echo   build_docs    - Build API docs
echo   build         - Build CLI binary
echo   test          - Run tests
echo   run           - Run the CLI
echo   build_wasm    - Build WASM version
echo   build_docker  - Build Docker images
echo   run_docker    - Run Docker image and ping JSON-RPC
goto end

:install_base
echo Assuming dotnet SDK is installed or managed by system
goto end

:install_deps
dotnet restore CddOpenApi.slnx
goto end

:build_docs
set DOCS_DIR=%2
if "%DOCS_DIR%"=="" set DOCS_DIR=docs
dotnet tool restore
dotnet docfx build docs/docfx.json -o %DOCS_DIR%
goto end

:build
set BIN_DIR=%2
if "%BIN_DIR%"=="" set BIN_DIR=bin\Release\net10.0\win-x64\publish
dotnet publish src\Cdd.OpenApi.Cli\Cdd.OpenApi.Cli.csproj -c Release -r win-x64 --self-contained -o %BIN_DIR%
goto end

:test
dotnet test CddOpenApi.slnx
goto end

:run
set BIN_DIR=bin\Release\net10.0\win-x64\publish
dotnet publish src\Cdd.OpenApi.Cli\Cdd.OpenApi.Cli.csproj -c Release -r win-x64 --self-contained -o %BIN_DIR%
%BIN_DIR%\cdd-csharp.exe %2 %3 %4 %5 %6 %7 %8 %9
goto end

:build_wasm
dotnet publish src\Cdd.OpenApi.Cli\Cdd.OpenApi.Cli.csproj -c Release -r browser-wasm -o bin\wasm\
goto end

:build_docker
docker build -t cdd-csharp-alpine -f alpine.Dockerfile .
docker build -t cdd-csharp-debian -f debian.Dockerfile .
goto end

:run_docker
docker run -d --name cdd-csharp-test -p 8085:8082 cdd-csharp-alpine
timeout /t 2 /nobreak >nul
curl -X POST -H "Content-Type: application/json" -d "{\"jsonrpc\": \"2.0\", \"method\": \"version\", \"id\": 1}" http://127.0.0.1:8085/
docker stop cdd-csharp-test
docker rm cdd-csharp-test
goto end

:end
endlocal
