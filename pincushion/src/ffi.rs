//! FFI-safe functions for pincushion.

use core::slice;

use safer_ffi::ffi_export;

use crate::{
    get_areas_in_place, points_to_icosahedrons_in_place, sample_points as sample_points_native,
    sample_triangles_in_place, scale_areas as scale_areas_native,
    set_points_from_sampled_triangles as set_points_from_sampled_triangles_native,
    vector3::{Vector3, Vector3U},
    Triangle, Uv, Vertex,
};

#[safer_ffi::derive_ReprC]
#[repr(C)]
pub struct Vec3 {
    pub x: f32,
    pub y: f32,
    pub z: f32,
}

impl Vector3 for Vec3 {
    fn x(&self) -> f32 {
        self.x
    }

    fn y(&self) -> f32 {
        self.y
    }

    fn z(&self) -> f32 {
        self.z
    }

    fn x_mut(&mut self) -> &mut f32 {
        &mut self.x
    }

    fn y_mut(&mut self) -> &mut f32 {
        &mut self.y
    }

    fn z_mut(&mut self) -> &mut f32 {
        &mut self.z
    }

    fn new(x: f32, y: f32, z: f32) -> Self {
        Self { x, y, z }
    }
}

#[derive(Copy, Clone)]
#[safer_ffi::derive_ReprC]
#[repr(C)]
pub struct Vec3U {
    pub x: usize,
    pub y: usize,
    pub z: usize,
}

impl Vector3U for Vec3U {
    fn x(&self) -> usize {
        self.x
    }

    fn y(&self) -> usize {
        self.y
    }

    fn z(&self) -> usize {
        self.z
    }

    fn x_mut(&mut self) -> &mut usize {
        &mut self.x
    }

    fn y_mut(&mut self) -> &mut usize {
        &mut self.y
    }

    fn z_mut(&mut self) -> &mut usize {
        &mut self.z
    }

    fn new(x: usize, y: usize, z: usize) -> Self {
        Self { x, y, z }
    }
}

/// - `vertices`: A flat vec of (x, y, z) vertices.
/// - `triangles`: A flat vec of three indices of vertices.
/// - `areas`: A vec that will be filled with the areas of each triangle in `triangles`.
///   This must be the same length as `triangles.len() / 3`.
///
/// Returns: The total area.
#[ffi_export]
pub fn get_areas(
    vertices: &safer_ffi::Vec<Vec3>,
    triangles: &safer_ffi::Vec<Vec3U>,
    areas: &mut safer_ffi::Vec<f32>,
) -> f32 {
    get_areas_in_place(vertices, triangles, areas)
}

/// Scale pre-calculated areas.
///
/// - `areas`: A slice that will be filled with the areas of each triangle
/// - `scale`: The uniform scale of the mesh.
#[ffi_export]
pub fn scale_areas(areas: &mut safer_ffi::Vec<f32>, scale: f32) -> f32 {
    scale_areas_native(areas, scale)
}

/// Sample random points on the mesh.
///
/// - `vertices`: A flat vec of (x, y, z) vertices.
/// - `triangles`: A flat vec of three indices of vertices.
/// - `areas`: The area of each triangle. See: `get_areas(vertices, triangles, areas)`
/// - `total_area`: The total area.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
///   This will be filled with the sampled points.
///   This must be defined on the other side of the FFI boundary.
///   To get the expected size of `points`, call `get_areas(vertices, triangles, areas)` followed by `get_num_points(total_area, points_per_m)`
#[ffi_export]
pub fn sample_points(
    vertices: &safer_ffi::Vec<Vec3>,
    triangles: &safer_ffi::Vec<Vec3U>,
    areas: &safer_ffi::Vec<f32>,
    total_area: f32,
    points: &mut safer_ffi::Vec<Vec3>,
) {
    sample_points_native(vertices, triangles, areas, total_area, points);
}

/// Sample random points in a mesh and generate a single mesh compose of icosahedrons.
///
/// - `vertices`: A flat vec of (x, y, z) vertices.
/// - `triangles`: A flat vec of three indices of vertices.
/// - `areas`: The area of each triangle. See: `get_areas(vertices, triangles, areas)`
/// - `total_area`: The total area.
/// - `radius`: The radius of each icosahedron.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
///   This will be filled with the sampled points.
///   This must be defined on the other side of the FFI boundary.
///   To get the expected size of `points`, call `get_areas(vertices, triangles, areas)` followed by `get_num_points(total_area, points_per_m)`
/// - `ico_vertices` The vertices of *all* icosahedrons in the mesh. Expected size: `points.len() * 12`.
/// - `ico_triangles` The triangle indices of *all* icosahedrons in the mesh. Expected size: `points.len() * 20`.
/// - `ico_uvs` The UVs of the vertices of *all* icosahedrons in the mesh. Expected size: `points.len() * 2`.
#[ffi_export]
pub fn points_to_icosahedrons(
    vertices: &safer_ffi::Vec<f32>,
    triangles: &safer_ffi::Vec<usize>,
    areas: &safer_ffi::Vec<f32>,
    total_area: f32,
    radius: f32,
    points: &mut safer_ffi::Vec<f32>,
    ico_vertices: &mut safer_ffi::Vec<f32>,
    ico_triangles: &mut safer_ffi::Vec<usize>,
    ico_uvs: &mut safer_ffi::Vec<f32>,
) {
    unsafe {
        // Sample the points.
        let vertices = ffi_vertices(vertices);
        let triangles = ffi_triangles(triangles);
        let points = ffi_vertices_mut(points);
        sample_points_native(vertices, triangles, areas, total_area, points);
        // Get icosahedra.
        let ico_vertices = ffi_vertices_mut(ico_vertices);
        let ico_triangles = ffi_triangles_mut(ico_triangles);
        let ico_uvs = ffi_uvs_mut(ico_uvs);
        points_to_icosahedrons_in_place(points, radius, ico_vertices, ico_triangles, ico_uvs);
    }
}

#[ffi_export]
pub fn sample_triangles(
    triangles: &safer_ffi::Vec<Vec3U>,
    areas: &safer_ffi::Vec<f32>,
    total_area: f32,
    sampled_triangles: &mut safer_ffi::Vec<Vec3U>,
) {
    sample_triangles_in_place(triangles, areas, total_area, sampled_triangles);
}

#[ffi_export]
pub fn set_points_from_sampled_triangles(
    vertices: &safer_ffi::Vec<Vec3>,
    sampled_triangles: &mut safer_ffi::Vec<Vec3U>,
    points: &mut safer_ffi::Vec<Vec3>,
) {
    set_points_from_sampled_triangles_native(vertices, sampled_triangles, points);
}

/// Converts a flat array of vertex coordinates from a safer-ffi vec into a shaped slice of vertices.
/// e.g.: `[x0, y0, z0, x1, y1, z1, ...]` into `[[x0, y0, z0], [x1, y1, z1], ...]`
unsafe fn ffi_vertices(vertices: &safer_ffi::Vec<f32>) -> &[Vertex] {
    slice::from_raw_parts(vertices.as_ptr() as *const Vertex, vertices.len() / 3)
}

/// Converts a flat array of vertex coordinates from a safer-ffi vec into a shaped slice of vertices.
/// e.g.: `[x0, y0, z0, x1, y1, z1, ...]` into `[[x0, y0, z0], [x1, y1, z1], ...]`
unsafe fn ffi_vertices_mut(vertices: &mut safer_ffi::Vec<f32>) -> &mut [Vertex] {
    slice::from_raw_parts_mut(vertices.as_mut_ptr() as *mut Vertex, vertices.len() / 3)
}

/// Converts a flat array of triangle indices from a safer-ffi vec into a shaped slice of triangles.
unsafe fn ffi_triangles(triangles: &safer_ffi::Vec<usize>) -> &[Triangle] {
    slice::from_raw_parts(triangles.as_ptr() as *const Triangle, triangles.len() / 3)
}

/// Converts a flat array of triangle indices from a safer-ffi vec into a shaped slice of triangles.
unsafe fn ffi_triangles_mut(triangles: &mut safer_ffi::Vec<usize>) -> &mut [Triangle] {
    slice::from_raw_parts_mut(triangles.as_mut_ptr() as *mut Triangle, triangles.len() / 3)
}

/// Converts a flat array of triangle indices from a safer-ffi vec into a shaped slice of triangles.
unsafe fn ffi_uvs_mut(uvs: &mut safer_ffi::Vec<f32>) -> &mut [Uv] {
    slice::from_raw_parts_mut(uvs.as_mut_ptr() as *mut Uv, uvs.len() / 2)
}
