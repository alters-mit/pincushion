use rand::{distributions::Uniform, rngs::ThreadRng, Rng};

use crate::{Triangle, Vertex};

use super::{sample_normal, sample_point, Sampler};

pub(crate) struct PointSampler<'mesh> {
    pub vertices: &'mesh [Vertex],
    pub normals: &'mesh [Vertex],
    pub sampled_points: &'mesh mut [Vertex],
    pub sampled_normals: &'mesh mut [Vertex],
    pub range: Uniform<f32>,
}

impl<'mesh> Sampler for PointSampler<'mesh> {
    fn sample(
        &mut self,
        triangle: &Triangle,
        point_index: usize,
        _: usize,
        _: usize,
        rng: &mut ThreadRng,
    ) {
        // Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
        let t = f32::sqrt(rng.sample(self.range));
        let u = 1. - t;
        let v = (1. - rng.sample(self.range)) * t;
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
