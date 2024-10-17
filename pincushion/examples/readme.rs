use tobj::{load_obj, GPU_LOAD_OPTIONS};

use pincushion::{sample_points_from_ppm, Triangle, Vertex};

fn get_obj(path: &str) -> (Vec<Vertex>, Vec<Triangle>) {
    let obj = &load_obj(path, &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
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

fn main() {
    let (vertices, triangles) = get_obj("tests/suzanne.obj");
    let points_per_m = 0.015;
    let scale = 1.;
    let _ = sample_points_from_ppm(&vertices, &triangles, points_per_m, scale);
}
