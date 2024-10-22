use std::{
    fs::write,
    time::{Duration, Instant},
};

use pincushion::{from_obj, sample_points_from_ppm, Triangle, Vertex};

pub fn main() {
    let (vertices, triangles, normals) = from_obj("tests/suzanne.obj");
    // Run the benchmark many times and average the result.
    let num_iterations = 1000;
    let dt = (0..num_iterations)
        .map(|_| benchmark(&vertices, &triangles, &normals))
        .sum::<Duration>()
        .as_micros()
        / num_iterations as u128;
    let text = format!("Sampling: {}μs", dt);
    write("../benchmark.txt", &text).unwrap();
    println!("{}", text);
}

fn benchmark(vertices: &[Vertex], triangles: &[Triangle], normals: &[Vertex]) -> Duration {
    let t0 = Instant::now();
    sample_points_from_ppm(80., vertices, triangles, normals);
    Instant::now() - t0
}
