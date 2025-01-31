use fastrand::Rng;

use crate::{Triangle, Vertex};

use super::{sample_normal, sample_point, Sampler};

pub(crate) struct PointSampler<'mesh> {
    pub vertices: &'mesh [Vertex],
    pub normals: &'mesh [Vertex],
    pub sampled_points: &'mesh mut [Vertex],
    pub sampled_normals: &'mesh mut [Vertex],
}

impl Sampler for PointSampler<'_> {
    fn sample(&mut self, triangle: &Triangle, point_index: usize, rng: &mut Rng) {
        // Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
        let t = f32::sqrt(rng.f32());
        let u = 1. - t;
        let v = (1. - rng.f32()) * t;
        let w = 1. - u - v;

        sample_point(
            &mut self.sampled_points[point_index],
            u,
            v,
            w,
            triangle,
            self.vertices,
        );
        sample_normal(
            &mut self.sampled_normals[point_index],
            triangle,
            self.normals,
        );
    }
}
