# Pincushion

**Uniform mesh sampler in Rust and Unity.**

![Suzanne test mesh on the left, and Suzanne as sampled points on the right.](doc/images/pincushion_banner.png)

## Overview

@ OVERVIEW @

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
  - **Deformable meshes (SkinnedMeshRenderer) are resampled in a shader.**[^1] Pincushion assumes that the mesh won't deform *too* much, and uses some cached data to make per-frame resampling *fast*.[^2]

**Additionally, Pincushion has unique rendering options not found in any other uniform mesh sampler example that I have seen thus far.**

## Repo structure

This repo has three components:

1. `pincushion` is a Rust library that sample points on a mesh. It has FFI-safe functions that can be used in C#.
2. `PincushionCs` contains native bindings for `pincushion`, Unity methods for sampling points and rendering them, and specialized shaders.
3. `UnityExample` is a small Unity example of Pincushion.

## How to add `Pincushion` to your Unity project

1. Download and install Rust
2. Within this repo, `cd pincushion` and `cargo build --release`
3. Copy the library into your Unity project's `Assets/` folder.[^3] It's located in: `pincushion/target/release/`
4. Copy the `PincushionCs/` folder into your Unity project
5. Project Settings -> Player -> Allow 'unsafe'  Code
6. Add a new GameObject with a `PincushionManager` component to the scene.
7. Assign the `Main Camera` in `PincushionManager`.
8. Set all other values as-needed:

![The PincushionManager Inspector panel.](doc/images/pincushion_manager.png)

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
| Render Mode | This controls how Pincushion is rendered (see below). |
| Texture | The texture of each point. Can be null, in which case a default texture is used. |
| Color | The color of each point. |
| Point Radius | The radius of each point in meters. |
| Constant Scaling | If true, every point will render at the same size. |
| Apply Mask | If true, apply a mask. A fraction of the sampled points defined by `Mask Factor` will be rendered. |
| Mask Factor | A factor between 0 and 1 that controls how many points will be skipped when rendering. |

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

### Examples

Example scenes are in `UnityExample/Assets/Scenes/`

- `SampleScene` has a basic Pincushion setup with two MeshRenderers.
- `ApplyMask` has a slider that you can drag to adjust the rendering mask.

## Usage (Rust)

Pincushion can alternatively be used in a native Rust context.

To add `pinchusion` to your project: `cargo add pincushion`.

Documentation for the Rust codebase can be found on [docs.rs](https://docs.rs/pincushion/latest/pincushion/).

### Example Usage

```rust
@ RUST_EXAMPLE @
```

@ RUST_DOC @

## Known limitations

- The native `pincushion` library must be compiled separately for each target platform (Windows, MacOS, Linux) and you have to compile on that platform (no cross-compilation like in Unity).
- I haven't tried Pincushion in WebGL but it probably doesn't work.
- To render a Pincushion mesh, the source mesh be readable (see Unity's documentation for mesh import options).
- `PincushionSkinnedMeshRenderer` has a suboptimal step that is somewhat slow.[^4] There is a better, faster way to do things, but Pincushion was built for an older project that uses Unity 2020. If I ever upgrade that project, I'll upgrade Pincushion too.[^5]

***

[^1]: We need to resample deformable meshes per-frame because the points need to move.

[^2]: Pincushion samples the indices of the triangles exactly once on the CPU and then per-frame on the GPU samples points from the vertices at those indices.

[^3]: Native libraries work like any other file in Unity. Stick it within `Assets/` or any subdirectory thereof and Unity will be able to find it.

[^4]: It's `BakeMesh(mesh)`, which copies data into a new mesh. Conceptually, we should be able to reference mesh vertex data (which is on the GPU) in a compute buffer (which is also on the GPU).

[^5]: Yes, I could use `#if` blocks to handle multiple Unity versions, but it ain't gonna happen until I need to.