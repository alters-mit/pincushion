# Pincushion

![Suzanne test mesh with a bunch of points on her face](suzanne.png)

Uniformly sample points on a mesh. 

This repo has three components:

1. `pincushion` is a Rust library that uses a very fast algorithm to sample points on a mesh. This can also be used to generate a mesh in which each sampled point is an icosahedron (20-sided die) within a single combined mesh.
2. `PincushionCs` contains native bindings for `pincushion` and Unity methods for sampling points and applying them to meshes. It also contains a shader for rendering points on a SkinnedMeshRenderer.
3. `UnityExample` is a small Unity example of Pincushion.

## How to add `Pincushion` to your Unity project

1. Download and install Rust
2. Within this repo, `cd pincushion` and `cargo build --release`
3. Copy the library into your Unity Project. It's located in: `pincushion/target/release/`
4. Copy the `PincushionCs/` folder into your Unity project.

## Usage (Unity)

### 1. Sample points

To sample points on a MeshRenderer, add a `PincushionStaticRenderer` component:

![A Pincushion Static Generator in the Inspector window](static_renderer.png)

| Parameter | Description |
| --- | --- |
| Points Per M | The number of sampled points per square meter on the mesh surface. |
| Point Radius | The radius of each point in meters. |
| Mode | Controls how the points are handled in the scene (see below). |
| Material | The material used to render each point. |

To sample points on a SkinnedMeshRenderer, add a `PincushionDynamicRenderer` component:

![A Pincushion Dynamic Generator in the Inspector window](dynamic_renderer.png)

| Parameter | Description |
| --- | --- |
| Points Per M | The number of sampled points per square meter on the mesh surface. |
| Point Radius | The radius of each point in meters. |
| Mode | Controls how the points are handled in the scene (see below). |
| Color | The color of each point. |

Creation modes:

| Mode | Description |
| --- | --- |
| Create | Create a new GameObject and mesh with sampled points. |
| Create and Hide Original | Create a new GameObject and mesh with sampled points. Keep the original GameObject but hide it. |
| Replace | Replace the original mesh with the sampled points mesh. No new GameObject is created. |


### 2. Show/hide the original/sampled mesh

Assuming you haven't chosen `Replace` for your creation mode (which doesn't create a new object), you can show/hide the original mesh or new mesh:

1. `PincushionRenderer pr = gameObject.GetComponent<PincushionRenderer>()`
2. To show/hide the original object: `pr.SetOriginalVisibility(show)` where `show` is a boolean.
3. To show/hide the the new object: `pr.SetMyVisibility(show)` where `show` is a boolean.

### 3. How it works

There are two methods of rendering sampled points because there is an efficient way to render points if we know that the mesh can't deform (i.e. if it is rendered via a MeshRenderer).

- MeshRenderers are sampled exactly once and then rendered as a mesh composed of multiple icosahedrons, one at each sampled point. See the summary tag in `PincushionStaticRenderer` for why this is reasonable.
- SkinnedMeshRenderers are sampled exactly once and rendered using a geometry shader.  See the summary tag in `PincushionDynamicRenderer` for why.

## Usage (Rust)

Pincushion can alternatively be used in a native Rust context.

To add `pinchusion` to your project: `cargo add pincushion`.

### Example Usage

```rust
@ RUST_EXAMPLE @
```

@ RUST_DOC @

Documentation for the Rust codebase can be found on [docs.rs](https://docs.rs/pincushion/latest/pincushion/).