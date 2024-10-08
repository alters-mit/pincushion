//! Given a target density, generate randomly sampled points on a mesh.
//!
//! This crate was built with Unity/C# bindings in mind. It's useful for sampling points for *static* meshes (i.e. not SkinnedMeshRenders).
//!
//! # Usage (native Rust)
//!
//! ```
//! use tobj::{load_obj, GPU_LOAD_OPTIONS};
//! 
//! use pincushion::sample_points_from_ppcm;
//!
//! fn get_obj(path: &str) -> (Vec<[f32; 3]>, Vec<[usize; 3]>) {
//!     let obj = &load_obj(path, &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
//!     let vertices = obj.positions.chunks_exact(3).map(|v| [v[0], v[1], v[2]]).collect::<Vec<[f32; 3]>>();
//!     let triangles = obj.indices.chunks_exact(3).map(|triangle|
//!         [triangle[0] as usize, triangle[1] as usize, triangle[2] as usize]
//!     ).collect::<Vec<[usize; 3]>>();
//!     (vertices, triangles)
//! }
//!
//! fn main() {
//!     let (vertices, triangles) = get_obj("tests/suzanne.obj");
//!
//!     // This, plus the volume, controls the number of points per centimeter.
//!     // The volume of the mesh is assumed to be in meters squared.
//!     let points_per_cm = 1.5;
//!
//!     // Sample the points.
//!     let points = sample_points_from_ppcm(&vertices, &triangles, points_per_cm);
//! }
//! ```
//! 
//! # Usage (Unity)
//! 
//! Add the C# scripts in this repo into your project. Then, add: `using Pincushion;`
//! 
//! To generate points, call `Vector3[] points = mesh.GetSampledPoints(pointsPerCm);` or `int size = mesh.GetSampledPoints(pointsPerCm, ref points);`.
//! 
//! The expected number of points is the product of the mesh volume and `pointsPerCm`.
//! If you don't include `numPoints`, the output will match the expected number.
//! If you do include `numPoints`, then `points.Length == numPoints`. This is the (slightly) faster option.
//!
//! To build a library that can be used in Unity/C#: `cargo build --release --features ffi`
//!
//! To generate C# native bindings: `cargo run --bin cs --features cs`
//!
//! # Example
//!
//! To run the example: `cargo run --example suzanne`

#[cfg(feature = "cs")]
pub mod cs;

#[cfg(feature = "ffi")]
pub mod ffi;

use mesh_rand::MeshSurface;
use rand::{prelude::Distribution, thread_rng};

/// Returns the signed volume of a triangle.
/// Source: https://stackoverflow.com/a/1568551
pub fn signed_volume_of_triangle(p0: &[f32; 3], p1: &[f32; 3], p2: &[f32; 3]) -> f32 {
    let v321 = p2[0] * p1[1] * p0[2];
    let v231 = p1[0] * p2[1] * p0[2];
    let v312 = p2[0] * p0[1] * p1[2];
    let v132 = p0[0] * p2[1] * p1[2];
    let v213 = p1[0] * p0[1] * p2[2];
    let v123 = p0[0] * p1[1] * p2[2];
    (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123)
}

/// - `vertices`: The vertices as a flat slice of coordinates.
/// - `triangles`: The triangles as a flat slice of indices.
///
/// Returns: The volume of the mesh.
pub fn get_volume(vertices: &[[f32; 3]], triangles: &[[usize; 3]]) -> f32 {
    triangles
        .iter()
        .map(|triangle| {
            signed_volume_of_triangle(
                &vertices[triangle[0]],
                &vertices[triangle[1]],
                &vertices[triangle[2]],
            )
        })
        .sum::<f32>()
        .abs()
}

/// Sample points on a mesh, given a density of points.
///
/// - `vertices`: The vertices.
/// - `triangles`: The triangle indices.
/// - `points_per_cm`: The number of points per centimeter.
///
/// Returns: An vec of sampled points.
pub fn sample_points_from_ppcm(
    vertices: &[[f32; 3]],
    triangles: &[[usize; 3]],
    points_per_cm: f32,
) -> Vec<[f32; 3]> {
    let volume = get_volume(vertices, triangles);
    let mut points = vec![[0.0; 3]; get_num_points(volume, points_per_cm)];
    sample_points(vertices, triangles, &mut points);
    points
}

/// Sample random points on the mesh.
///
/// - `vertices`: The vertices.
/// - `triangles`: The triangle indices.
/// - `points`: A pre-defined slice of vertices that will be filled with points.
pub fn sample_points(vertices: &[[f32; 3]], triangles: &[[usize; 3]], points: &mut [[f32; 3]]) {
    let surface = MeshSurface::new(vertices, triangles).unwrap();
    let mut rng = thread_rng();
    surface
        .sample_iter(&mut rng)
        .take(points.len())
        .zip(points.iter_mut())
        .for_each(|(sample, point)| *point = sample.position);
}

/// - `volume`: The volume of the mesh, assumed to be in meters squared.
/// - `points_per_cm`: The number of points per centimeter.
///
/// Returns: The number of points that should be sampled.
pub fn get_num_points(volume: f32, points_per_cm: f32) -> usize {
    (volume * 100.0 * points_per_cm) as usize
}

#[cfg(test)]
mod tests {
    use tobj::{load_obj, GPU_LOAD_OPTIONS};

    use crate::{get_volume, sample_points_from_ppcm};

    #[test]
    fn test_volume() {
        let (vertices, triangles) = get_obj();
        let volume = get_volume(&vertices, &triangles);
        assert_eq!(volume as usize, 2);
    }

    #[test]
    fn test_sample_points() {
        let (vertices, triangles) = get_obj();
        let points = sample_points_from_ppcm(&vertices, &triangles, 1.0);
        assert_eq!(points.len(), 259);
    }

    fn get_obj() -> (Vec<[f32; 3]>, Vec<[usize; 3]>) {
        let obj = &load_obj("tests/suzanne.obj", &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
        let vertices = obj
            .positions
            .chunks_exact(3)
            .map(|v| [v[0], v[1], v[2]])
            .collect::<Vec<[f32; 3]>>();
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
            .collect::<Vec<[usize; 3]>>();
        (vertices, triangles)
    }
}
