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
