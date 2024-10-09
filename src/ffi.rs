use core::slice;

use safer_ffi::ffi_export;

use crate::{get_triangle_areas_in_place, sample_points as sample_points_native};

#[ffi_export]
pub fn get_areas(
    vertices: &safer_ffi::Vec<f32>,
    triangles: &safer_ffi::Vec<usize>,
    areas: &mut safer_ffi::Vec<f32>,
) -> f32 {
    unsafe {
        let vertices = ffi_vertices(vertices);
        let triangles = ffi_triangles(triangles);
        get_triangle_areas_in_place(vertices, triangles, areas)
    }
}

/// Sample random points on the mesh.
///
/// - `vertices`: The vertices as a flat slice of coordinates.
/// - `triangles`: The triangles as a flat slice of indices.
/// - `points`: A pre-defined flat slice of coordinates.
///   This will be filled with the sampled pointsc.
///   This must be defined on the other side of the FFI boundary.
///   To get the expected size of `points`, call `get_volume_ffi(vertices, triangles)` followed by `get_num_points_ffi(volume, points_per_cm)`
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
unsafe fn ffi_vertices(vertices: &safer_ffi::Vec<f32>) -> &[[f32; 3]] {
    slice::from_raw_parts(vertices.as_ptr() as *const [f32; 3], vertices.len() / 3)
}

/// Converts a flat array of triangle indices from a safer-ffi vec into a shaped slice of triangles.
unsafe fn ffi_triangles(triangles: &safer_ffi::Vec<usize>) -> &[[usize; 3]] {
    slice::from_raw_parts(triangles.as_ptr() as *const [usize; 3], triangles.len() / 3)
}

/// Converts a flat array of vertex coordinates from a safer-ffi vec into a shaped slice of vertices.
/// e.g.: `[x0, y0, z0, x1, y1, z1, ...]` into `[[x0, y0, z0], [x1, y1, z1], ...]`
unsafe fn ffi_vertices_mut(vertices: &mut safer_ffi::Vec<f32>) -> &mut [[f32; 3]] {
    slice::from_raw_parts_mut(vertices.as_mut_ptr() as *mut [f32; 3], vertices.len() / 3)
}
