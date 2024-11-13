# Pincushion

**Uniform mesh sampler in Rust and Unity.**

![Suzanne test mesh on the left, and Suzanne as sampled points on the right.](doc/images/pincushion_banner.png)

## Overview

- What if you wanted to render a little dot at every vertex of a mesh because it looks cool?
- Hmm those dots sure are bunching up around complex geometry. Like, there are way more dots on the ears than on the legs.
- It would look a lot better if the dots were *uniformly* (evenly) (sorta) dispersed around the mesh.
- Within each patch of similar-sized surface area, we'll add a dot. For spice, the position within that patch of area is randomly *sampled*.

## What makes Pincushion special

There are other uniform mesh sampling in Unity but (as far as I know) the all freely-available example has at least one of the following problems:

- The example uses HDRP, which is no good for older projects (such as the ones I work on)
- The example uses inefficient C# code to sample points
- The example includes only MeshRenderers or only SkinnedMeshRenderers

Pincushion solves these problems with some unusual/novel features:

- **Pincushion works in Unity's built-in render pipeline.**
- **Points are sampled using native Rust code.** This is, as they say, blazingly fast, and *much* faster than C#.
- Pincushion can handle both static meshes (MeshRenderers) and deformable meshes (SkinnedMeshRenderers). 
  - **Static meshes (MeshRenderer) are sampled only once.** This is *very* fast.
  - **Deformable meshes (SkinnedMeshRenderer) use cached data.**[^1] Pincushion assumes that the mesh won't deform *too* much, and uses some cached data to make per-frame resampling *fast*.[^2]

**Additionally, Pincushion has unique rendering options not found in any other uniform mesh sampler example that I have seen thus far.**

## Repo structure

This repo has three components:

1. `pincushion` is a Rust library that sample points on a mesh. It has FFI-safe functions that can be used in C#.
2. `PincushionCs` contains native bindings for `pincushion`, Unity methods for sampling points and rendering them, and specialized shaders.
3. `UnityExample` is a small Unity example of Pincushion.

## How to add `Pincushion` to your Unity project

1. Download and install Rust
2. Within this repo, `cd pincushion` and `cargo build --release`[^3]
3. Copy the library into your Unity project's `Assets/` folder.[^4] It's located in: `pincushion/target/release/`
4. Copy the `PincushionCs/` folder into your Unity project
5. Project Settings -> Player -> Allow 'unsafe'  Code
6. Add a new GameObject with a `PincushionManager` component to the scene.
7. Assign the `PincushionManager a `Main Camera`.
8. Set all other values as-needed:

![The PincushionManager Inspector panel.](doc/images/pincushion_manager.png)

| Parameter | Description |
| --- | --- |
| Main Camera | The camera that will render Pincushion. |
| Set Background Color | If true, the background of the scene will be Background Color. If false, the background won't change. |
| Background Color | The solid color of the background. |
| Source Meshes Layer Name | All source mesh objects will be set to this layer. |
| Sampled Meshes Layer Name | All sampled mesh objects will be set to this layer. |
| Points Per M | The number of sampled points per square meter on the mesh surface. |
| Multiply Points Per M By Camera Distance | If true, multiply the number of points by the object's initial distance from the camera. |
| Multiply Points Per M By Object Scale | If true, multiply the number of points by the object's initial uniform scale. |
| Render Mode | This controls how Pincushion is rendered (see below). |
| Texture | The texture of each point. |
| Color | The color of each point. |
| Point Radius | The radius of each point in meters. |
| Constant Scaling | If true, every point will render at the same size. |

### Render Modes

`Do Not` will render the scene as-is. This is useful if you want to toggle back and forth between the original rendering and pincushion rendering:

![The Do Not rendering mode. There are no dots.](doc/images/do_not.png)

`With Source Meshes` will render the sampled point as well as the original (source) meshes:


![The With Source Meshes rendering mode. There are dots on the meshes.](doc/images/with_source_meshes.png)

`Without Source Meshes` will render the sampled points and hide the source meshes:

![The Without Source Meshes rendering mode. There are dots and only dots.](doc/images/without_source_meshes.png)

`Hide Backfacing` is will render the sampled points and hide the source meshes. Points facing away from the camera will be hidden:

![The Hide Backfacing rendering mode. There are dots and only dots, but not the dots facing away from the camera.](doc/images/hide_backfacing.png)

`Occlude Behind` is will render the sampled points and hide the source meshes. Points will be occluded as if the source mesh was rendered in front of them:

![The Occlude Behind rendering mode. There are dots and only dots, but many of the dots are occluded.](doc/images/occlude_behind.png)

### Reinitialize

You can reinitialize Pincushion by doing the following:

1. Set parameters as-needed, like this: `PincushionManager.Instance.pointsPerM = 90f;`
2. Call Set(): `PincushionManager.Set()`

This works for all `PincushionManager` parameters, including the Render Mode.

## Usage (Rust)

Pincushion can alternatively be used in a native Rust context.

To add `pinchusion` to your project: `cargo add pincushion`.

Documentation for the Rust codebase can be found on [docs.rs](https://docs.rs/pincushion/latest/pincushion/).

### Example Usage

```rust
use pincushion::Mesh;

fn main() {
    // Add feature "obj" to enable `from_obj`.
    let mesh = Mesh::from_obj("tests/suzanne.obj");
    let points_per_m = 80.;
    let scale = 1.;
    let _ = mesh.sample_points(points_per_m, scale);
}

```

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

***

[^1]: We need to resample deformable meshes per-frame because the points need to move.

[^2]: See benchmarks! Points code also be resampled with a compute shader but it's *very* fast as-is. I haven't benchmarked compute shader resampling yet but my guess is that the overhead dispatching the shader to and from the CPU might make it slower than the Rust implementation.

[^3]: You will need to do this for each platform you intend to develop on. Rust cannot cross-compile like Unity can.

[^4]: Native libraries work like any other file in Unity. Stick it within `Assets/` or any subdirectory thereof and Unity will be able to find it.