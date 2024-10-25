use rand::{distributions::Uniform, rngs::ThreadRng, Rng};

use crate::{Triangle, Vertex};

use super::{sample_normal, sample_point, Sampler};

pub(crate) struct PointSampler<'mesh> {
    pub vertices: &'mesh [Vertex],
    pub normals: &'mesh [Vertex],
    pub sampled_points: &'mesh mut [Vertex],
    pub sampled_normals: &'mesh mut [Vertex],
    pub range: &'mesh Uniform<f32>,
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
        sample_point(
            &mut self.sampled_points[point_index],
            rng.sample(self.range),
            rng.sample(self.range),
            &triangle,
            &self.vertices,
        );
        sample_normal(
            &mut self.sampled_normals[point_index],
            triangle,
            self.normals,
        );
    }
}
