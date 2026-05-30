# cdd-csharp WASM Support

The `cdd-csharp` CLI tool is compatible with WebAssembly (WASM).

By using the `Wasi.Sdk` package, this CLI can be fully compiled and distributed as a pure WASI package.

## Building for WASM

Run the provided Make task:
```bash
make build_wasm
```

### Important Build Requirements
To ensure the WASM build is correctly packaged and compatible with the browser runtime (e.g., for `cdd-web-ui`), the build process **must**:
1. Include the `-r browser-wasm` flag during `dotnet publish` to instruct the compiler to generate WebAssembly artifacts.
2. Package the `AppBundle/_framework` directory rather than the raw `publish` output directory. `AppBundle` correctly generates the `blazor.boot.json` metadata file and configures the native WASM binaries which are required by the .NET WebAssembly runtime. Failing to do this will result in missing file errors like `Failed to load config file ./blazor.boot.json`.

## Concurrency in Web Workers

When running `cdd-csharp` in a browser Web Worker (e.g., within `cdd-web-ui`), ensure the .NET initialization (`dotnet.create()`) is strictly controlled to avoid race conditions.

If the worker receives multiple messages simultaneously, it might attempt to initialize the .NET runtime concurrently, leading to:
```text
Error: Runtime module already loaded
```
This is particularly an issue in Firefox and WebKit. To prevent this, wrap the WASM boot process in a global promise so that `dotnet.create()` is only invoked once per worker context, and subsequent calls wait for the initial promise to resolve.

## Important Note on Stack Size

When running `cdd-csharp.wasm` in a JavaScript engine (such as Node.js or a web browser), you may occasionally encounter a `Maximum call stack size exceeded` (or similar Stack Overflow) exception during startup or execution.

This is a known behavior of the embedded `Microsoft.CodeAnalysis.CSharp` (Roslyn) compiler, which the CLI uses internally to parse and generate C# code. Specifically, the runtime uses deep recursion within WASM reflection, syntax tree analysis, and type instantiation inside Roslyn when loading standard libraries inside the WebAssembly VM, which exceed the default v8 call stack depth of ~10,000 frames.

**Note:** We have completely eliminated AST recursion during formatting by permanently restoring and integrating `WasmSafeFormatter`. Code generation now relies exclusively on iterative, stack-based token traversal (`WasmSafeFormatter.Format`) instead of Roslyn's native `.NormalizeWhitespace()`, which was causing "Execution failed: too much recursion" exceptions in browser WebAssembly environments when processing large schemas.

**Workaround for Node.js:**
Increase the stack size limit significantly by passing the `--stack-size` flag when running Node.js (e.g. at least 100000):
```bash
node --stack-size=100000 --experimental-wasi-unstable-preview1 your_wasi_script.mjs
```

**Workaround for Browsers:**
Browsers typically do not allow overriding the maximum WASM call stack size. If integrating `cdd-csharp` into a web IDE (like `cdd-web-ui`), consider executing the WASM module inside a dedicated Web Worker and monitor its memory limits, or offload the execution to a lightweight Node.js backend configured with a higher stack size limit.
