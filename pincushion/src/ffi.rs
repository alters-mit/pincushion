//! FFI-safe functions for pincushion.

use core::slice;

use safer_ffi::ffi_export;

use crate::{get_areas_in_place, sample_points as sample_points_native, Triangle, Vertex};

/// - `vertices`: A flat vec of (x, y, z) vertices.
/// - `triangles`: A flat vec of three indices of vertices.
/// - `areas`: A vec that will be filled with the areas of each triangle in `triangles`.
///   This must be the same length as `triangles.len() / 3`.
///
/// Returns: The total area.
#[ffi_export]
pub fn get_areas(
    vertices: &safer_ffi::Vec<f32>,
    triangles: &safer_ffi::Vec<usize>,
    areas: &mut safer_ffi::Vec<f32>,
) -> f32 {
    unsafe {
        let vertices = ffi_vertices(vertices);
        let triangles = ffi_triangles(triangles);
        get_areas_in_place(vertices, triangles, areas)
    }
}

/// Sample random points on the mesh.
///
/// - `vertices`: A flat vec of (x, y, z) vertices.
/// - `triangles`: A flat vec of three indices of vertices.
/// - `areas`: The area of each triangle. See: `get_areas(vertices, triangles, areas)`
/// - `total_area`: The total area.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
///   This will be filled with the sampled pointsc.
///   This must be defined on the other side of the FFI boundary.
///   To get the expected size of `points`, call `get_areas(vertices, triangles, areas)` followed by `get_num_points(total_area, points_per_m)`
#[ffi_export]
pub fn sample_points(
    vertices: &safer_ffi::Vec<f32>,
    triangles: &safer_ffi::Vec<usize>,
    areas: &safer_ffi::Vec<f32>,
    total_area: f32,
    points: &mut safer_ffi::Vec<f32>,
) {
    unsafe {
        let vertices = ffi_vertices(vertices);
        let triangles = ffi_triangles(triangles);
        let points = ffi_vertices_mut(points);
        sample_points_native(vertices, triangles, areas, total_area, points);
    }
}

/// Converts a flat array of vertex coordinates from a safer-ffi vec into a shaped slice of vertices.
/// e.g.: `[x0, y0, z0, x1, y1, z1, ...]` into `[[x0, y0, z0], [x1, y1, z1], ...]`
unsafe fn ffi_vertices(vertices: &safer_ffi::Vec<f32>) -> &[Vertex] {
    slice::from_raw_parts(vertices.as_ptr() as *const Vertex, vertices.len() / 3)
}

/// Converts a flat array of triangle indices from a safer-ffi vec into a shaped slice of triangles.
unsafe fn ffi_triangles(triangles: &safer_ffi::Vec<usize>) -> &[Triangle] {
    slice::from_raw_parts(triangles.as_ptr() as *const Triangle, triangles.len() / 3)
}

/// Converts a flat array of vertex coordinates from a safer-ffi vec into a shaped slice of vertices.
/// e.g.: `[x0, y0, z0, x1, y1, z1, ...]` into `[[x0, y0, z0], [x1, y1, z1], ...]`
unsafe fn ffi_vertices_mut(vertices: &mut safer_ffi::Vec<f32>) -> &mut [Vertex] {
    slice::from_raw_parts_mut(vertices.as_mut_ptr() as *mut Vertex, vertices.len() / 3)
}
