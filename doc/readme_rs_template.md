### Features

- `ffi` is enabled by default, and adds the FFI-safe functions that allow Pincushion to interface with C#. If you're planning to use Pincushion in a Rust-only project, you should remove this flag, as doing so will let Pincushion use SIMD-capable structs instead of FFI-safe structs. For example, removing the `ffi` features replaces `safer_ffi::Vec<Vertex>` with `Vec<glam::Vec3A>`.
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

To run the example:

```sh
cargo run --example suzanne --features obj
```

### Benchmarks

To run the benchmark:

```sh
cargo bench benchmark --features obj
```

Results:

@ BENCHMARKS @