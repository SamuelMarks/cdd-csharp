# cdd-csharp WASM Support

The `cdd-csharp` CLI tool is compatible with WebAssembly (WASM).

By using the `Wasi.Sdk` package, this CLI can be fully compiled and distributed as a pure WASI package.

## Building for WASM

Run the provided Make task:
```bash
make build_wasm
```

The output will be placed in `bin/cdd-csharp.wasm`. This output is a standalone WASI preview 1 binary that can be run with tools like `wasmtime` or in the browser using a WASI polyfill.
This enables `cdd-csharp` to be used in a unified CLI of all `cdd-*` projects without requiring `dotnet` to be installed on the host, or integrated into a web-based IDE entirely client-side.

## Important Note on Stack Size

When running `cdd-csharp.wasm` in a JavaScript engine (such as Node.js or a web browser), you may encounter a `Maximum call stack size exceeded` (or similar Stack Overflow) exception during startup or execution.

This is a known behavior of the embedded `Microsoft.CodeAnalysis.CSharp` (Roslyn) compiler, which the CLI uses internally to parse and generate C# code. Roslyn's static initializers build deep syntax trees that exceed the default call stack limit of many JS engines (like V8, which defaults to ~1MB or 10,000 frames).

**Workaround for Node.js:**
Increase the stack size limit by passing the `--stack-size` flag when running Node.js:
```bash
node --stack-size=10000 --experimental-wasi-unstable-preview1 your_wasi_script.js
```

**Workaround for Browsers:**
Browsers typically do not allow overriding the maximum WASM call stack size. If integrating `cdd-csharp` into a web IDE (like `cdd-web-ui`), consider executing the WASM module inside a dedicated Web Worker and monitor its memory limits, or offload the execution to a lightweight Node.js backend configured with a higher stack size limit.
