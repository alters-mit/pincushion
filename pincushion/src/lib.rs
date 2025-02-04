//! Uniform mesh sampler for Rust and Unity.
//!
#![doc = include_str!("../../doc/overview.md")]
//!
//! Pincushion can be used as a typical Rust crate or as a native library in Unity.
//!
//! This documentation is for the Rust crate.
//! Documentation for Unity/C# can be found [here](https://github.com/alters-mit/pincushion).
//!
//! ### Usage
//!
//! ```
#![doc = include_str!("../examples/readme.rs")]
//! ```
//!
#![doc = include_str!("../../doc/readme_rs.md")]

use std::slice::from_raw_parts_mut;

use glam::{Mat4, Vec3};
use safer_ffi::ffi_export;

pub use area::Area;
pub use mesh::Mesh;
pub use vecs::*;

#[cfg(feature = "cs")]
pub mod cs;
#[cfg(feature = "mask")]
pub mod mask;

mod area;
mod mesh;
mod sampler;
mod vecs;

/// - `mesh` The source mesh.
/// - `scale` The uniform scale of the mesh.
/// - `area`: The `Area` of the mesh.
#[ffi_export]
pub fn set_area(mesh: &Mesh, scale: f32, area: &mut Area) {
    mesh.set_area(scale, area)
}

/// - `total_area`: The total area of the triangles in square meters. See: `set_area(mesh, scale, area)`.
/// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
///
/// Returns: The number of points to be sampled.
#[ffi_export]
pub fn get_num_points(total_area: f32, points_per_m: f32) -> usize {
    (total_area * points_per_m) as usize
}

/// Sample random points on the mesh.
///
/// - `mesh` The source mesh.
/// - `area`: The `Area` of the mesh.
/// - `sampled_points`: (x, y, z) sampled points.
/// - `sampled_normals`: Normal directional vectors, one per sampled point. This must be the same size as `sampled_points`.
/// - `seed`: A random seed used for sampling.
#[ffi_export]
pub fn sample_points(
    mesh: &Mesh,
    area: &Area,
    sampled_points: &mut safer_ffi::Vec<Vertex>,
    sampled_normals: &mut safer_ffi::Vec<Vertex>,
    seed: u64,
) {
    mesh.set_sampled_points(area, sampled_points, sampled_normals, Some(seed));
}

/// Set the triangles at which points can be sampled.
/// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
///
/// - `mesh` The source mesh.
/// - `area`: The `Area` of the mesh.
/// - `sampled_triangles`: A pre-defined slice of triangles that will be set in this function. The size must match the number of points that will be sampled.
/// - `seed`: A random seed used for sampling.
#[ffi_export]
pub fn sample_triangles(
    mesh: &Mesh,
    area: &Area,
    sampled_triangles: &mut safer_ffi::Vec<Triangle>,
    seed: u64,
) {
    mesh.set_sampled_triangles(area, sampled_triangles, Some(seed));
}

/// Apply a transform matrix to transform sampled points.
/// This is meant to be used in a Unity context to transform a mesh by a position and rotation.
///
/// - `matrix`: A 4x4 transform matrix. The length is assumed to always be 16.
/// - `points`: The points that will be transformed.
#[ffi_export]
pub fn transform_points(matrix: &safer_ffi::Vec<f32>, points: &mut safer_ffi::Vec<Vertex>) {
    let matrix = Mat4::from_cols_slice(matrix);

    // Cast `points` as `glam::Vec3`.
    // This is very, very safe because both structs are repr(C).
    let ptr = points.as_mut_ptr().cast::<Vec3>();
    let len = points.len();
    unsafe {
        // Cast, and then transform.
        from_raw_parts_mut(ptr, len)
            .iter_mut()
            .for_each(|p| *p = matrix.transform_point3(*p));
    }
}
