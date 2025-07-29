pub(crate) mod point_sampler;
pub(crate) mod triangle_sampler;

use crate::{Area, Triangle};
use fastrand::Rng;

/// A trait used to sample points or triangles.
pub(crate) trait Sampler {
    fn sample(&mut self, triangle: &Triangle, point_index: usize, rng: &mut Rng);

    fn sample_points(
        &mut self,
        area: &Area,
        num_points: usize,
        triangles: &[Triangle],
        seed: Option<u64>,
    ) {
        // The area per point is used to uniformly sample the points.
        let area_per_point = area.total_area / num_points as f32;
        let mut rng = match seed {
            Some(seed) => Rng::with_seed(seed),
            None => Rng::new(),
        };
        // When sampling points, start at this index.
        let mut start_index_point = 0;
        // When choosing random triangles, start at this index.
        let mut start_index_triangle = 0;
        // The accumulated triangle area. This is used to set the end indices.
        let mut total_accumulated_area = 0.0;
        for (area_index, area) in area.areas.iter().enumerate() {
            // Add area.
            total_accumulated_area += *area;
            // We have enough area.
            if total_accumulated_area >= area_per_point {
                // Derive how many points we can fit in the accumulated area.
                let num_points_in_area_f = (total_accumulated_area / area_per_point).ceil();
                let mut num_points_in_area = num_points_in_area_f as usize;
                // Clamp to avoid exceeding the total number of points.
                if start_index_point + num_points_in_area >= num_points {
                    num_points_in_area = num_points - start_index_point;
                }
                // Sample some points.
                // If there is only one triangle, then sample the points in that triangle repeatedly.
                let range = start_index_point..start_index_point + num_points_in_area;
                if start_index_triangle == area_index {
                    let triangle = &triangles[start_index_triangle];
                    range.for_each(|point_index| self.sample(triangle, point_index, &mut rng));
                }
                // If there are multiple triangles, get a Uniform distribution (for efficiency) and randomly select triangles.
                else {
                    range.for_each(|point_index| {
                        let triangle = &triangles[rng.usize(start_index_triangle..=area_index)];
                        self.sample(triangle, point_index, &mut rng);
                    });
                }
                // Start adding points at the offset.
                start_index_point += num_points_in_area;
                // Reset the accumulated area, keeping the remainder.
                total_accumulated_area -= area_per_point * num_points_in_area_f;
                // Increment to the next starting triangle.
                start_index_triangle = area_index + 1;
            }
        }
    }
}
