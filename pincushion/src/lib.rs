//! Uniformly sample points on a mesh.
//!
//! Includes FFI-safe functions for Unity/C# bindings.
//!
//! ### Usage
//!
//! ```
#![doc = include_str!("../examples/readme.rs")]
//! ```
//!
//! Documentation for Unity/C# can be found [here](https://github.com/alters-mit/pincushion).
//!
#![doc = include_str!("../../readme_rs.md")]

use rand::{distributions::Uniform, thread_rng, Rng};

use vecs::{Vector3, Vector3U};

#[cfg(feature = "cs")]
pub mod cs;

#[cfg(feature = "ffi")]
pub mod ffi;

pub mod vecs;

pub type Vertex = [f32; 3];
pub type Triangle = [usize; 3];

/// - `vertices`: (x, y, z) vertices.
/// - `triangles`: Indices of vertices comprising a triangle.
///
/// Returns: The area of each triangle and the total surface area of the mesh in square meters.
pub fn get_areas<T, U>(vertices: &[T], triangles: &[U]) -> (Vec<f32>, f32)
where
    T: Vector3,
    U: Vector3U,
{
    let mut areas = vec![0.0; triangles.len()];
    let total_area = get_areas_in_place(vertices, triangles, &mut areas);
    (areas, total_area)
}

/// Scale pre-calculated areas.
///
/// - `areas`: A slice that will be filled with the areas of each triangle.
/// - `scale`: The uniform scale of the mesh.
pub fn scale_areas(areas: &mut [f32], scale: f32) -> f32 {
    areas.iter_mut().for_each(|a| *a *= scale);
    areas.iter().sum::<f32>()
}

/// - `vertices`: (x, y, z) vertices.
/// - `triangles`: Indices of vertices comprising a triangle.
/// - `areas`: A slice that will be filled with the areas of each triangle. This must be the same length as `triangles`.
///
/// Returns: The total surface area of the mesh in square meters.
pub fn get_areas_in_place<T, U>(vertices: &[T], triangles: &[U], areas: &mut [f32]) -> f32
where
    T: Vector3,
    U: Vector3U,
{
    let mut total_area = 0.;
    triangles
        .iter()
        .zip(areas.iter_mut())
        .for_each(|(triangle, area)| {
            // Get this triangle's area.
            let a = Vector3::get_triangle_area(
                &vertices[triangle.x()],
                &vertices[triangle.y()],
                &vertices[triangle.z()],
            );
            // Add to the total.
            total_area += a;
            *area = a;
        });
    total_area
}

/// - `total_area`: The total area of the triangles in square meters. See: `get_areas(vertices, triangles)` and `get_areas_in_place(vertices, triangles, areas)`
/// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
///
/// Returns: The number of points to be sampled.
#[cfg_attr(feature = "ffi", safer_ffi::ffi_export)]
pub fn get_num_points(total_area: f32, points_per_m: f32) -> usize {
    (total_area * points_per_m) as usize
}

/// Sample points on a mesh, given a density of points.
///
/// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
/// - `vertices`:  (x, y, z) vertices.
/// - `triangles`:Indices of vertices comprising a triangle.
///
/// Returns: An vec of sampled points.
pub fn sample_points_from_ppm<T, U>(points_per_m: f32, vertices: &[T], triangles: &[U]) -> Vec<T>
where
    T: Vector3,
    U: Vector3U,
{
    let (areas, total_area) = get_areas(vertices, triangles);
    let num_points = get_num_points(total_area, points_per_m);
    let mut points = vec![T::new(0., 0., 0.); num_points];
    sample_points(total_area, vertices, triangles, &areas, &mut points);
    points
}

/// Sample random points on the mesh.triangle_end_index
///
/// - `total_area`: The total surface area of the mesh in square meters.
/// - `vertices`: (x, y, z) vertices.
/// - `triangles`: Indices of vertices comprising a triangle.
/// - `areas`: A slice that will be filled with the areas of each triangle. This must be the same length as `triangles`.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
pub fn sample_points<T, U>(
    total_area: f32,
    vertices: &[T],
    triangles: &[U],
    areas: &[f32],
    points: &mut [T],
) where
    T: Vector3,
    U: Vector3U,
{
    // The area per point is used to uniformly sample the points.
    let area_per_point = total_area / points.len() as f32;
    let mut rng = thread_rng();
    // When sampling points, start at this index.
    let mut start_index_point = 0;
    // When choosing trandom triangles, start at this index.
    let mut start_index_triangle = 0;
    // The accumulated triangle area. This is used to set the end indices.
    let mut total_accumulated_area = 0.0;
    let range = Uniform::new(0., 1.);
    for (index, area) in areas.iter().enumerate() {
        // Add area.
        total_accumulated_area += *area;
        // We have enough area.
        if total_accumulated_area >= area_per_point {
            // Derive how many points we can fit in the accumulated area.
            let num_points = (total_accumulated_area / area_per_point) as usize;
            // Sample some points.
            for i in 0..num_points {
                // Get a random triangle, bounded by the start index and the current index in `areas`.
                let triangle = if start_index_point == index {
                    &triangles[start_index_point]
                } else {
                    &triangles[rng.gen_range(start_index_triangle..=index)]
                };
                // Get a random point on that triangle.
                set_point(
                    &mut points[start_index_point + i],
                    rng.sample(range),
                    rng.sample(range),
                    vertices,
                    triangle,
                );
            }
            // Start adding points at the offset.
            start_index_point += num_points;
            // Reset the accumulated area.
            total_accumulated_area = 0.0;
            // Increment to the next starting triangle.
            start_index_triangle = index + 1;
        }
    }
}

/// Set the triangles at which points can be sampled.
/// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
///
/// - `total_area`: The total surface area of the mesh in square meters.
/// - `triangles`: Indices of vertices comprising a triangle.
/// - `areas`: A slice that will be filled with the areas of each triangle. This must be the same length as `triangles`.
/// - `sampled_triangles`: A pre-defined slice of triangles that will be set in this function. The size can differ from `triangles` and `areas` and must match the number of points that will be sampled.
pub fn sample_triangles_in_place<T>(
    total_area: f32,
    triangles: &[T],
    areas: &[f32],
    sampled_triangles: &mut [T],
) where
    T: Vector3U,
{
    // The area per point is used to uniformly sample the points.
    let area_per_point = total_area / sampled_triangles.len() as f32;
    let mut rng = thread_rng();
    // When sampling points, start at this index.
    let mut start_index_point = 0;
    // When choosing trandom triangles, start at this index.
    let mut start_index_triangle = 0;
    // The accumulated triangle area. This is used to set the end indices.
    let mut total_accumulated_area = 0.0;
    for (index, area) in areas.iter().enumerate() {
        // Add area.
        total_accumulated_area += *area;
        // We have enough area.
        if total_accumulated_area >= area_per_point {
            // Derive how many points we can fit in the accumulated area.
            let num_points = (total_accumulated_area / area_per_point) as usize;
            // Sample some points.
            for i in 0..num_points {
                // Get a random triangle, bounded by the start index and the current index in `areas`.
                sampled_triangles[start_index_point + i] = if start_index_point == index {
                    triangles[start_index_point]
                } else {
                    triangles[rng.gen_range(start_index_triangle..=index)]
                };
            }
            // Start adding points at the offset.
            start_index_point += num_points;
            // Reset the accumulated area.
            total_accumulated_area = 0.0;
            // Increment to the next starting triangle.
            start_index_triangle = index + 1;
        }
    }
}

/// Get the triangles at which points can be sampled.
/// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
///
/// - `total_area`: The total surface area of the mesh in square meters.
/// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
/// - `triangles`: Indices of vertices comprising a triangle.
/// - `areas`: A slice that will be filled with the areas of each triangle. This must be the same length as `triangles`.
///
/// Returns the sampled triangles.
pub fn sample_triangles<T>(
    total_area: f32,
    points_per_m: f32,
    triangles: &[T],
    areas: &[f32],
) -> Vec<T>
where
    T: Vector3U,
{
    let mut samples = vec![T::new(0, 0, 0); get_num_points(total_area, points_per_m)];
    sample_triangles_in_place(total_area, triangles, areas, &mut samples);
    samples
}

/// Given pre-sampled triangles, sample vertices.
/// The position of the vertex relative to the spatial area of the triangle is deterministic.
/// In constrast, points sampled via `sample_points` and `sample_points_ppm` will be at a random point on a sampled triangle.
///
/// - `vertices`: (x, y, z) vertices.
/// - `normals`: (x, y, z) normals.
/// - `sampled_triangles`: Presampled triangles.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size must be the same as `sampled_triangles`.
/// - `sampled_normals`: A pre-defined slice of normal vectors per point in `points`.
pub fn set_points_from_sampled_triangles<T, U>(
    vertices: &[T],
    normals: &[T],
    sampled_triangles: &[U],
    points: &mut [T],
    sampled_normals: &mut [T],
) where
    T: Vector3,
    U: Vector3U,
{
    points
        .iter_mut()
        .zip(sampled_triangles.iter().zip(sampled_normals.iter_mut()))
        .for_each(|(point, (triangle, normal))| {
            set_point(point, 0.5, 0.5, vertices, triangle);
            set_normal(normal, normals, triangle);
        });
}

/// Load a .obj file.
///
/// Returns: The vertices, the triangles, and the normals.
#[cfg(feature = "obj")]
pub fn from_obj<P>(path: P) -> (Vec<Vertex>, Vec<Triangle>, Vec<Vertex>)
where
    P: AsRef<std::path::Path> + std::fmt::Debug,
{
    let obj = &tobj::load_obj(path, &tobj::GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
    let vertices = obj
        .positions
        .chunks_exact(3)
        .map(|v| [v[0], v[1], v[2]])
        .collect::<Vec<Vertex>>();
    let triangles = obj
        .indices
        .chunks_exact(3)
        .map(|triangle| {
            [
                triangle[0] as usize,
                triangle[1] as usize,
                triangle[2] as usize,
            ]
        })
        .collect::<Vec<Triangle>>();
    let normals = obj
        .normals
        .chunks_exact(3)
        .map(|normal| [normal[0], normal[1], normal[2]])
        .collect::<Vec<Vertex>>();
    (vertices, triangles, normals)
}

/// Get a point on a triangle.
/// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
fn set_point<T, U>(point: &mut T, u: f32, v: f32, vertices: &[T], triangle: &U)
where
    T: Vector3,
    U: Vector3U,
{
    let t = f32::sqrt(v);
    let v = u * t;
    let u = (1.0 - u) * t;
    let w = 1.0 - u - v;
    // Set the point at `start_index_pooint` offset by 0..num_points.
    *point = vertices[triangle.x()]
        .mul(u)
        .add(&vertices[triangle.y()].mul(v))
        .add(&vertices[triangle.z()].mul(w));
}

/// Set the average normal of a triangle.
fn set_normal<T, U>(normal: &mut T, normals: &[T], triangle: &U)
where
    T: Vector3,
    U: Vector3U,
{
    *normal = normals[triangle.x()]
        .add(&normals[triangle.y()])
        .add(&normals[triangle.z()])
        .div(3.)
}

#[cfg(test)]
mod tests {
    #[cfg(feature = "obj")]
    #[test]
    fn test_sample_points() {
        let (vertices, triangles, _) = super::from_obj("tests/suzanne.obj");
        let points = super::sample_points_from_ppm(80., &vertices, &triangles);
        assert_eq!(points.len(), 997);
    }
}
