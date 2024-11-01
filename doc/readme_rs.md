### Features

- `obj` adds a `Mesh::from_obj(path)` function to load a mesh from a .obj file.
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

Sample points: 44μs

Sample triangles: 28μs

Sample points from pre-sampled triangles: 7μs