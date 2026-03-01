@echo off
setlocal enabledelayedexpansion

if "%~1"=="" goto help
if /I "%~1"=="help" goto help
if /I "%~1"=="all" goto help
if /I "%~1"=="install_base" goto install_base
if /I "%~1"=="install_deps" goto install_deps
if /I "%~1"=="build_docs" goto build_docs
if /I "%~1"=="build" goto build
if /I "%~1"=="build_wasm" goto build_wasm
if /I "%~1"=="test" goto test
if /I "%~1"=="run" goto run

echo Unknown command: %~1
goto help

:install_base
echo Installing .NET SDK...
winget install Microsoft.DotNet.SDK.9 --accept-source-agreements --accept-package-agreements
if errorlevel 1 (
    winget install Microsoft.DotNet.SDK.Preview --accept-source-agreements --accept-package-agreements
)
if errorlevel 1 (
    echo winget failed or not found, please install .NET SDK manually from https://dotnet.microsoft.com/download
)
goto :eof

:install_deps
dotnet restore
goto :eof

:build_docs
set "DOCS_DIR=%~2"
if "%DOCS_DIR%"=="" set "DOCS_DIR=docs"
dotnet build src\Cdd.OpenApi\Cdd.OpenApi.csproj -c Release
if not exist "%DOCS_DIR%" mkdir "%DOCS_DIR%"
copy src\Cdd.OpenApi\bin\Release\*\Cdd.OpenApi.xml "%DOCS_DIR%" >nul 2>&1
if errorlevel 1 echo No XML docs generated.
goto :eof

:build
set "BIN_DIR=%~2"
if "%BIN_DIR%"=="" set "BIN_DIR=bin"
dotnet publish src\Cdd.OpenApi.Cli\Cdd.OpenApi.Cli.csproj -c Release -o "%BIN_DIR%"
goto :eof

:build_wasm
set "BIN_DIR=%~2"
if "%BIN_DIR%"=="" set "BIN_DIR=bin"
echo Installing WASM workloads...
dotnet workload install wasm-tools
dotnet workload install wasi-experimental
echo Building for WASI/WASM...
dotnet publish src\Cdd.OpenApi.Cli\Cdd.OpenApi.Cli.csproj -c Release -r browser-wasm -o "%BIN_DIR%\wasm"
goto :eof

:test
dotnet test
goto :eof

:run
dotnet publish src\Cdd.OpenApi.Cli\Cdd.OpenApi.Cli.csproj -c Release -o "bin"
set "ARGS="
shift
:run_args_loop
if "%~1"=="" goto run_args_done
set "ARGS=!ARGS! %1"
shift
goto run_args_loop
:run_args_done
bin\cdd-csharp.exe !ARGS!
goto :eof

:help
echo Available tasks:
echo   install_base   - install language runtime and anything else relevant (e.g., .NET SDK)
echo   install_deps   - install local dependencies (dotnet restore)
echo   build_docs     - build the API docs and put them in the "docs" directory. Usage: make.bat build_docs [dir]
echo   build          - build the CLI binary. Usage: make.bat build [dir]
echo   test           - run tests locally
echo   run            - the CLI. Dependencies are built first. Usage: make.bat run [args...]
echo   help           - show what options are available
echo   all            - show help text
goto :eof
