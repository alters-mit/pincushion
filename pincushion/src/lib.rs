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
use vector3::Vector3U;

use crate::vector3::Vector3;

#[cfg(feature = "cs")]
pub mod cs;

#[cfg(feature = "ffi")]
pub mod ffi;

pub mod vector3;

pub type Vertex = [f32; 3];
pub type Triangle = [usize; 3];
pub type Uv = [f32; 2];

const NUM_ICOSAHEDRON_VERTICES: usize = 12;
const NUM_ICOSAHEDRON_TRIANGLES: usize = 20;

/// - `vertices`: A slice of (x, y, z) vertices.
/// - `triangles`: A slice of three indices of vertices.
///
/// Returns: The area of each triangle and the total area.
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
/// - `areas`: A slice that will be filled with the areas of each triangle
/// - `scale`: The uniform scale of the mesh.
pub fn scale_areas(areas: &mut [f32], scale: f32) -> f32 {
    areas.iter_mut().for_each(|a| *a *= scale);
    areas.iter().sum::<f32>()
}

/// - `vertices`: A slice of (x, y, z) vertices.
/// - `triangles`: A slice of three indices of vertices.
/// - `areas`: A slice that will be filled with the areas of each triangle in `triangles`.
///   This must be the same length as `triangles`.
///
/// Returns: The total area.
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
/// - `points_per_m`: The number of points per square meter.
///
/// Returns: The number of points to be sampled.
#[cfg_attr(feature = "ffi", safer_ffi::ffi_export)]
pub fn get_num_points(total_area: f32, points_per_m: f32) -> usize {
    (total_area * points_per_m) as usize
}

/// Sample points on a mesh, given a density of points.
///
/// - `vertices`: A slice of (x, y, z) vertices.
/// - `triangles`: A slice of three indices of vertices.
/// - `points_per_m`: The number of points per square meter.
///
/// Returns: An vec of sampled points.
pub fn sample_points_from_ppm<T, U>(vertices: &[T], triangles: &[U], points_per_m: f32) -> Vec<T>
where
    T: Vector3 + Clone,
    U: Vector3U,
{
    let (areas, total_area) = get_areas(vertices, triangles);
    let num_points = get_num_points(total_area, points_per_m);
    let mut points = vec![T::new(0., 0., 0.); num_points];
    sample_points(vertices, triangles, &areas, total_area, &mut points);
    points
}

/// Sample random points on the mesh.triangle_end_index
///
/// - `vertices`: A slice of (x, y, z) vertices.
/// - `triangles`: A slice of three indices of vertices.
/// - `areas`: The area of each triangle. See: [`get_areas(vertices, triangles)`] and [`get_areas_in_place(vertices, triangles, areas)`].
/// - `total_area`: The total area.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
pub fn sample_points<T, U>(
    vertices: &[T],
    triangles: &[U],
    areas: &[f32],
    total_area: f32,
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

pub fn sample_triangles_in_place<T>(
    triangles: &[T],
    areas: &[f32],
    total_area: f32,
    sampled_triangles: &mut [T],
) where
    T: Vector3U + Copy + Clone,
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

pub fn sample_triangles<T>(
    triangles: &[T],
    areas: &[f32],
    total_area: f32,
    points_per_m: f32,
) -> Vec<T>
where
    T: Vector3U + Copy + Clone,
{
    let mut samples = vec![T::new(0, 0, 0); get_num_points(total_area, points_per_m)];
    sample_triangles_in_place(triangles, areas, total_area, &mut samples);
    samples
}

pub fn set_points_from_sampled_triangles<T, U>(
    vertices: &[T],
    sampled_triangles: &[U],
    points: &mut [T],
) where
    T: Vector3,
    U: Vector3U,
{
    points
        .iter_mut()
        .zip(sampled_triangles)
        .for_each(|(point, triangle)| {
            set_point(point, 0.5, 0.5, vertices, triangle);
        });
}

/// Convert a slice of (x, y, z) points into a single mesh composed of multiple icosahedrons (20-sided die).
///
/// - `points` The sampled points.
/// - `radius` The radius of each icosahedron.
/// - `vertices` The vertices of *all* icosahedrons in the mesh. Expected size: `points.len() * 12`.
/// - `triangles` The triangle indices of *all* icosahedrons in the mesh. Expected size: `points.len() * 20`.
/// - `uvs` The (u, v) coordinates of each icosahedron vertex.
pub fn points_to_icosahedrons_in_place(
    points: &[Vertex],
    radius: f32,
    vertices: &mut [Vertex],
    triangles: &mut [Triangle],
    uvs: &mut [Uv],
) {
    // Golden ratio.
    #[allow(clippy::excessive_precision)]
    const PHI: f32 = 1.618033988749894848204586834365638118;
    // The triangle indices in a icosahedron.
    // Source: https://superhedralcom.wordpress.com/2020/05/17/building-the-unit-icosahedron/
    const TRIANGLES: [Triangle; NUM_ICOSAHEDRON_TRIANGLES] = [
        [0, 11, 5],
        [0, 5, 1],
        [0, 1, 7],
        [0, 7, 10],
        [0, 10, 11],
        [1, 5, 9],
        [5, 11, 4],
        [11, 10, 2],
        [10, 7, 6],
        [7, 1, 8],
        [3, 9, 4],
        [3, 4, 2],
        [3, 2, 6],
        [3, 6, 8],
        [3, 8, 9],
        [4, 9, 5],
        [2, 4, 11],
        [6, 2, 10],
        [8, 6, 7],
        [9, 8, 1],
    ];
    // The vertices of an unit-sized icosahedron.
    // Source: https://superhedralcom.wordpress.com/2020/05/17/building-the-unit-icosahedron/
    const VERTICES: [Vertex; NUM_ICOSAHEDRON_VERTICES] = [
        [-1., PHI, 0.],
        [1., PHI, 0.],
        [-1., -PHI, 0.],
        [1., -PHI, 0.],
        [0., -1., PHI],
        [0., 1., PHI],
        [0., -1., -PHI],
        [0., 1., -PHI],
        [PHI, 0., -1.],
        [PHI, 0., 1.],
        [-PHI, 0., -1.],
        [-PHI, 0., 1.],
    ];
    // Source: https://www.alexisgiard.com/icosahedron-sphere/
    // See also: dev_code/uvs.py
    const UVS: [Uv; NUM_ICOSAHEDRON_VERTICES] = [
        [0.19193012, 1.0],
        [0.0881041, 1.0],
        [0.19193012, 0.5],
        [0.0881041, 0.5],
        [0.1762082, 0.56116754],
        [0.1762082, 0.8],
        [0.0, 0.56116754],
        [0.0, 0.8],
        [0.030034214, 0.6666667],
        [0.10825957, 0.6666667],
        [0.25, 0.6666667],
        [0.25, 0.6666667],
    ];

    let t = radius * PHI;
    // Scale the vertices.
    let mut ico_vertices = VERTICES;
    ico_vertices.iter_mut().for_each(|v| v.mul_mut(t));

    // Fill with initial values.
    let points_len = points.len();
    let mut vs = vec![ico_vertices; points_len];
    let mut ts = vec![TRIANGLES; points_len];
    // The UVs never change. Fill immediately.
    uvs.copy_from_slice(vec![UVS; points_len].as_flattened());

    points
        .iter()
        .enumerate()
        .zip(vs.iter_mut().zip(ts.iter_mut()))
        .for_each(|((i, point), (verts, tris))| {
            // Set the positions of the vertices.
            verts.iter_mut().for_each(|v| v.add_mut(point));
            // Increment the indices.
            let offset = i * NUM_ICOSAHEDRON_VERTICES;
            tris.iter_mut().flatten().for_each(|t| *t += offset);
        });
    // Copy into final arrays.
    vertices.copy_from_slice(vs.as_flattened());
    triangles.copy_from_slice(ts.as_flattened());
}

/// Convert a slice of (x, y, z) points into a single mesh composed of multiple icosahedrons (20-sided die).
///
/// - `points` The sampled points.
/// - `radius` The radius of each icosahedron.
///
/// Returns: A flat vec of the vertices of *all* vertices, triangles, and UVs.
pub fn points_to_icosahedrons(
    points: &[Vertex],
    radius: f32,
) -> (Vec<Vertex>, Vec<Triangle>, Vec<Uv>) {
    let length = points.len();
    let mut vertices = vec![[0.; 3]; NUM_ICOSAHEDRON_VERTICES * length];
    let mut triangles = vec![[0; 3]; NUM_ICOSAHEDRON_TRIANGLES * length];
    let mut uvs = vec![[0.; 2]; NUM_ICOSAHEDRON_VERTICES * length];
    points_to_icosahedrons_in_place(points, radius, &mut vertices, &mut triangles, &mut uvs);
    (vertices, triangles, uvs)
}

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

#[cfg(test)]
mod tests {
    use tobj::{load_obj, GPU_LOAD_OPTIONS};

    use crate::{
        points_to_icosahedrons, sample_points_from_ppm, Triangle, Vertex,
        NUM_ICOSAHEDRON_TRIANGLES, NUM_ICOSAHEDRON_VERTICES,
    };

    #[test]
    fn test_sample_points() {
        let (vertices, triangles) = get_obj();
        let points = sample_points_from_ppm(&vertices, &triangles, 0.015);
        assert_eq!(points.len(), 831);
    }

    #[test]
    fn test_icosahedra() {
        let (vertices, triangles) = get_obj();
        let points = sample_points_from_ppm(&vertices, &triangles, 0.015);
        let (ico_vertices, ico_triangles, _) = points_to_icosahedrons(&points, 0.02);
        let num_ico_vertices = ico_vertices.iter().flatten().count();
        assert_eq!(ico_vertices.len(), points.len() * NUM_ICOSAHEDRON_VERTICES);
        assert_eq!(
            ico_triangles.len(),
            points.len() * NUM_ICOSAHEDRON_TRIANGLES
        );
        ico_triangles
            .iter()
            .flatten()
            .for_each(|i| assert!(*i < num_ico_vertices, "{} {}", i, num_ico_vertices));
    }

    fn get_obj() -> (Vec<Vertex>, Vec<Triangle>) {
        let obj = &load_obj("tests/suzanne.obj", &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
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
        (vertices, triangles)
    }
}
