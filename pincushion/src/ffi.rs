//! FFI-safe functions for pincushion.

use safer_ffi::ffi_export;

use crate::{vecs::*, Mesh};

/// - `mesh` The source mesh.
/// - `scale` The uniform scale of the mesh.
/// - `areas`: The area of each triangle in the mesh.
#[ffi_export]
pub fn set_area(mesh: &Mesh, scale: f32, areas: &mut safer_ffi::Vec<f32>) -> f32 {
    mesh.set_area(scale, areas)
}

/// Sample random points on the mesh.
///
/// - `mesh` The source mesh.
/// - `total_area`: The total surface area of the mesh in square meters.
/// - `areas`: The area of each triangle in the mesh.
/// - `sampled_points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
/// - `sampled_normals`: A pre-defined slice that will be filled with normals. The size must match that of `points`.
#[ffi_export]
pub fn sample_points(
    mesh: &Mesh,
    total_area: f32,
    areas: &safer_ffi::Vec<f32>,
    sampled_points: &mut safer_ffi::Vec<Vertex>,
    sampled_normals: &mut safer_ffi::Vec<Vertex>,
) {
    mesh.set_sampled_points(total_area, areas, sampled_points, sampled_normals);
}

/// Set the triangles at which points can be sampled.
/// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
///
/// - `mesh` The source mesh.
/// - `total_area`: The total surface area of the mesh in square meters.
/// - `areas`: The area of each triangle in the mesh.
/// - `sampled_triangles`: The triangles that will be sampled.
#[ffi_export]
pub fn sample_triangles(
    mesh: &Mesh,
    total_area: f32,
    areas: &safer_ffi::Vec<f32>,
    sampled_triangles: &mut safer_ffi::Vec<Triangle>,
) {
    mesh.set_sampled_triangles(total_area, areas, sampled_triangles);
}

/// Given pre-sampled triangles, sample vertices.
/// The position of the vertex relative to the spatial area of the triangle is deterministic.
/// In contrast, points sampled via `sample_points` and `sample_points_ppm` will be at a random point on a sampled triangle.
///
/// - `mesh` The source mesh.
/// - `sampled_mesh`: The sampled mesh, which contains pre-sampled triangles.
#[ffi_export]
pub fn set_points_from_sampled_triangles(mesh: &Mesh, sampled_mesh: &mut Mesh) {
    mesh.set_presampled_mesh(sampled_mesh);
}
