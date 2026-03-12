# Pincushion

**Uniform mesh sampler for Rust and Unity3D.**

- What if you wanted to render a little dot at every vertex of a mesh because it looks cool?
- Hmm those dots sure are bunching up around complex geometry. Like, there are way more dots on the ears than on the legs.
- It would look a lot better if the dots were *uniformly* (evenly) (sorta) dispersed around the mesh.
- Within each patch of similar-sized surface area, we'll add a dot. For spice, the position within that patch of area is randomly *sampled*.

Pincushion can be used as a typical Rust crate or as a native library in Unity.

This documentation is for the Rust crate.
Documentation for Unity/C# can be found [here](https://github.com/alters-mit/pincushion).

### Usage

```rust
use pincushion::Mesh;

fn main() {
    // Add feature "obj" to enable `from_obj`.
    let mesh = Mesh::from_obj("tests/suzanne.obj");
    let points_per_m = 80.;
    let scale = 1.;
    let seed = Some(0);
    let _ = mesh.sample_points(points_per_m, scale, seed);
}
```

### Features

- `ffi` is enabled by default, and adds the FFI-safe functions that allow Pincushion to interface with C#. If you're planning to use Pincushion in a Rust-only project, you should remove this flag, as doing so will let Pincushion use SIMD-capable structs instead of FFI-safe structs. For example, removing the `ffi` features replaces `safer_ffi::Vec<Vertex>` with `Vec<glam::Vec3A>`.
- `obj` adds a `Mesh::from_obj(path)` function to load a mesh from a .obj file.
- `cs` should only be enabled when generating the C# code (see below).

### Create C# Native Bindings

The `com.mit.pincushion` C# package calls the native `pincushion` Rust library via auto-generated native binding methods.

To regenerate the native bindings:

```sh
cargo run --bin cs --features cs
```

The file will be at `../com.mit.pincushion/NativeBindings.cs`

### Example

To run the example:

```sh
cargo run --example suzanne --features obj
```

### Benchmark

To benchmark:

```sh
cargo bench --features obj
```

This will benchmark `pincushion`  with the `ffi` flag enabled. If the `ffi` flag is disabled, `pincushion` runs slightly faster. To benchmark without the `ffi` feature:

```sh
cargo bench --no-default-features --features obj
```
