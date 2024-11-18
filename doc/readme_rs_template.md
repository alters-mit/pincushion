### Features

- `obj` adds a `Mesh::from_obj(path)` function to load a mesh from a .obj file.
- `mask` adds a few FFI-safe functions to apply a "mask", showing/hiding some elements in an array. This is meant to be used in Unity and probably isn't useful elsewhere. 
- `cs` should only be enabled when generating the C# code (see below).

### Create C# Native Bindings

The `PincushionCs` code can call the native `pincushion` Rust library via auto-generated native binding methods.

To regenerate the native bindings:

```sh
cargo run --bin cs --features cs
```

The file will be at `../PincushionCs/NativeBindings.cs`

### Example

To run the example: `cargo run --example suzanne --features obj`

### Benchmarks

To run the benchmark: `cargo bench benchmark --features obj`

Results:

@ BENCHMARKS @