use rand::{rngs::ThreadRng, thread_rng, Rng};

use crate::{Area, Triangle, Vertex};

pub(crate) mod point_sampler;
pub(crate) mod triangle_sampler;

/// A trait used to sample points or triangles.
pub(crate) trait Sampler {
    fn sample(
        &mut self,
        triangle: &Triangle,
        point_index: usize,
        start_index_point: usize,
        i: usize,
        rng: &mut ThreadRng,
    );

    fn sample_points(&mut self, area: &Area, num_points: usize, triangles: &[Triangle]) {
        // The area per point is used to uniformly sample the points.
        let area_per_point = area.total_area / num_points as f32;
        let mut rng = thread_rng();
        // When sampling points, start at this index.
        let mut start_index_point = 0;
        // When choosing trandom triangles, start at this index.
        let mut start_index_triangle = 0;
        // The accumulated triangle area. This is used to set the end indices.
        let mut total_accumulated_area = 0.0;
        for (index, area) in area.areas.iter().enumerate() {
            // Add area.
            total_accumulated_area += *area;
            // We have enough area.
            if total_accumulated_area >= area_per_point {
                // Derive how many points we can fit in the accumulated area.
                let num_points = (total_accumulated_area / area_per_point) as usize;
                // Sample some points.
                for i in 0..num_points {
                    // Get a random triangle, bounded by the start index and the current index in `areas`.
                    let triangle = if start_index_triangle == index {
                        &triangles[start_index_triangle]
                    } else {
                        &triangles[rng.gen_range(start_index_triangle..=index)]
                    };
                    let point_index = start_index_point + i;
                    self.sample(triangle, point_index, start_index_point, i, &mut rng);
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
}

/// Get a point on a triangle.
/// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
pub(crate) fn sample_point(
    point: &mut Vertex,
    u: f32,
    v: f32,
    w: f32,
    triangle: &Triangle,
    vertices: &[Vertex],
) {
    *point = vertices[triangle.a]
        .mul(u)
        .add(&vertices[triangle.b].mul(v))
        .add(&vertices[triangle.c].mul(w));
}

/// Set the average normal of a triangle.
pub(crate) fn sample_normal(normal: &mut Vertex, triangle: &Triangle, normals: &[Vertex]) {
    *normal = normals[triangle.a]
        .add(&normals[triangle.b])
        .add(&normals[triangle.c])
        .div(3.)
}
