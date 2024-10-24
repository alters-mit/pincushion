//! FFI-safe functions for pincushion.

use safer_ffi::ffi_export;

use crate::{vecs::*, Area, Mesh};

/// - `mesh` The source mesh.
/// - `scale` The uniform scale of the mesh.
/// - `area`: The `Area` of the mesh.
#[ffi_export]
pub fn set_area(mesh: &Mesh, scale: f32, area: &mut Area) {
    mesh.set_area(scale, area)
}

/// Sample random points on the mesh.
///
/// - `mesh` The source mesh.
/// - `area`: The `Area` of the mesh.
/// - `sampled_points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
/// - `sampled_normals`: A pre-defined slice that will be filled with normals. The size must match that of `points`.
#[ffi_export]
pub fn sample_points(
    mesh: &Mesh,
    area: &Area,
    sampled_points: &mut safer_ffi::Vec<Vertex>,
    sampled_normals: &mut safer_ffi::Vec<Vertex>,
) {
    mesh.set_sampled_points(area, sampled_points, sampled_normals);
}

/// Set the triangles at which points can be sampled.
/// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
///
/// - `mesh` The source mesh.
/// - `area`: The `Area` of the mesh.
/// - `sampled_triangles`: The triangles that will be sampled.
#[ffi_export]
pub fn sample_triangles(
    mesh: &Mesh,
    area: &Area,
    sampled_triangles: &mut safer_ffi::Vec<Triangle>,
) {
    mesh.set_sampled_triangles(area, sampled_triangles);
}

/// Given pre-sampled triangles, sample vertices.
/// The position of the vertex relative to the spatial area of the triangle is deterministic.
///
/// - `mesh` The source mesh.
/// - `sampled_mesh`: The sampled mesh, which contains pre-sampled triangles.
#[ffi_export]
pub fn set_points_from_sampled_triangles(mesh: &Mesh, sampled_mesh: &mut Mesh) {
    mesh.set_presampled_mesh(sampled_mesh);
}
