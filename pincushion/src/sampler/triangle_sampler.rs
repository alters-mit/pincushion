use rand::rngs::ThreadRng;

use crate::Triangle;

use super::Sampler;

pub(crate) struct TriangleSampler<'mesh> {
    pub sampled_triangles: &'mesh mut [Triangle],
}

impl<'mesh> Sampler for TriangleSampler<'mesh> {
    fn sample(
        &mut self,
        triangle: &Triangle,
        _: usize,
        start_index_point: usize,
        i: usize,
        _: &mut ThreadRng,
    ) {
        self.sampled_triangles[start_index_point + i] = *triangle;
    }
}
