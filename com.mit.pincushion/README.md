# Pincushion

**Uniform mesh sampler in Rust and Unity.**

## Overview

- What if you wanted to render a little dot at every vertex of a mesh because it looks cool?
- Hmm those dots sure are bunching up around complex geometry. Like, there are way more dots on the ears than on the legs.
- It would look a lot better if the dots were *uniformly* (evenly) (sorta) dispersed around the mesh.
- Within each patch of similar-sized surface area, we'll add a dot. For spice, the position within that patch of area is randomly *sampled*.

[Changelog is here](changelog.md)

## What makes Pincushion special

There are other Unity uniform mesh sampling libraries but (as far as I know) the all freely-available example has at least one of the following problems:

- The example uses HDRP, which is no good for older projects (such as the ones I work on)
- The example uses inefficient C# code to sample points
- The example includes only MeshRenderers or only SkinnedMeshRenderers

Pincushion solves these problems with some unusual/novel features:

- **Pincushion works in Unity's built-in render pipeline.**
- **Points are sampled using native Rust code.** This is, as they say, blazingly fast, and *much* faster than C#.
- Pincushion can handle both static meshes (MeshRenderers) and deformable meshes (SkinnedMeshRenderers). 
  - **Static meshes (MeshRenderer) are sampled only once.** This is *very* fast.
  - **Deformable meshes (SkinnedMeshRenderer) are resampled in a shader.**[^1] Pincushion assumes that the mesh won't deform *too* much, and uses some cached data to make per-frame resampling *fast*.[^2]

**Additionally, Pincushion has unique rendering options not found in any other uniform mesh sampler example that I have seen thus far.**

## Repo structure

This repo has three components:

1. `pincushion` is a Rust library that sample points on a mesh. It has FFI-safe functions that can be used in C#.
2. `com.mit.pinsushion` is a Unity package, which includes binds for the native `pincushion` library, C#-friendly classes and methods, shaders, etc.
3. `UnityExample` is a small Unity example of Pincushion.

## How to add `Pincushion` to your Unity project

1. Allow 'unsafe' code: Project Settings -> Player -> Allow 'unsafe' Code
2. Open the package manager.

3. Click the + sign and select "Add package from git URL..."

4. Enter this URL:

```
https://github.com/alters-mit/pincushion.git?path=com.mit.pincushion
```

5. Add a new GameObject with a `PincushionManager` component to the scene.
6. Assign the `Main Camera` in `PincushionManager`.
7.  Set all other values as-needed.

| Parameter | Description |
| --- | --- |
| Auto Update | If true, Pincushion will update on Update(). If false, you must manually call ManuallyUpdate() to update Pincushion. |
| Main Camera | The camera that will render Pincushion. |
| Set Background Color | If true, the background of the scene will be Background Color. If false, the background won't change. |
| Background Color | The solid color of the background. |
| Source Meshes Layer Name | All source mesh objects will be set to this layer. |
| Sampled Meshes Layer Name | All sampled mesh objects will be set to this layer. |
| Ignore Meshes Layer Name | Meshes in this layer won't be rendered. |
| Points Per M | The number of sampled points per square meter on the mesh surface. |
| Multiply Points Per M By Camera Distance | If true, multiply the number of points by the object's initial distance from the camera. |
| Multiply Points Per M By Object Scale | If true, multiply the number of points by the object's initial uniform scale. |
| Auto Seed | If true, generate a new random seed every time a mesh is sampled. |
| Render Mode | This controls how Pincushion is rendered (see below). |
| Texture | The texture of each point. Can be null, in which case a default texture is used. |
| Color | The color of each point. |
| Point Radius | The radius of each point in meters. |
| Constant Scaling | If true, every point will render at the same size. |
| Apply Mask | If true, apply a mask. A fraction of the sampled points defined by `Mask Factor` will be rendered. |
| Mask Factor | A factor between 0 and 1 that controls how many points will be skipped when rendering. |

### Render Modes

`Do Not` will render the scene as-is. This is useful if you want to toggle back and forth between the original rendering and pincushion rendering:

`With Source Meshes` will render the sampled point as well as the original (source) meshes:

`Without Source Meshes` will render the sampled points and hide the source meshes:

`Hide Backfacing` is will render the sampled points and hide the source meshes. Points facing away from the camera will be hidden:

`Occlude Behind` is will render the sampled points and hide the source meshes. Points will be occluded as if the source mesh was rendered in front of them:

### Reinitialize

You can reinitialize Pincushion by doing the following:

1. Set parameters as-needed, like this: `PincushionManager.Instance.pointsPerM = 90f;`
2. Call Set(): `PincushionManager.Set()`

This works for all `PincushionManager` parameters, including the Render Mode.

### Examples

Example scenes are in `UnityExample/Assets/Scenes/`

- `SampleScene` has a basic Pincushion setup with two MeshRenderers..
- `ApplyMask` has a slider that you can drag to adjust the rendering mask.

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
    let seed = Some(0);
    let _ = mesh.sample_points(points_per_m, scale, seed);
}

```

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

Sample points: 24μs

Sample triangles: 17μs

Transformed points: 0.8395μs

## Known limitations

- Pincushion doesn't work in WebGL.
- To render a Pincushion mesh, the source mesh be readable (see Unity's documentation for mesh import options).
- `PincushionSkinnedMeshRenderer` has a suboptimal step that is somewhat slow.[^4] There is a better, faster way to do things, but Pincushion was built for an older project that uses Unity 2020. If I ever upgrade that project, I'll upgrade Pincushion too.[^5]

***

[^1]: We need to resample deformable meshes per-frame because the points need to move.

[^2]: Pincushion samples the indices of the triangles exactly once on the CPU and then per-frame on the GPU samples points from the vertices at those indices.

[^3]: Native libraries work like any other file in Unity. Stick it within `Assets/` or any subdirectory thereof and Unity will be able to find it.

[^4]: It's `BakeMesh(mesh)`, which copies data into a new mesh. Conceptually, we should be able to reference mesh vertex data (which is on the GPU) in a compute buffer (which is also on the GPU).

[^5]: Yes, I could use `#if` blocks to handle multiple Unity versions, but it ain't gonna happen until I need to.