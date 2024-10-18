use std::{
    fs::write,
    time::{Duration, Instant},
};

use tobj::{load_obj, GPU_LOAD_OPTIONS};

use pincushion::{sample_points_from_ppm, Triangle, Vertex};

pub fn main() {
    let (vertices, triangles) = get_obj();
    // Run the benchmark many times and average the result.
    let num_iterations = 1000;
    let dt = (0..num_iterations)
        .map(|_| benchmark(&vertices, &triangles))
        .sum::<Duration>()
        .as_micros()
        / num_iterations as u128;
    let text = format!("Sampling: {}μs", dt);
    write("../benchmark.txt", &text).unwrap();
    println!("{}", text);
}

fn benchmark(vertices: &[Vertex], triangles: &[Triangle]) -> Duration {
    let t0 = Instant::now();
    sample_points_from_ppm(80., &vertices, &triangles);
    Instant::now() - t0
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
