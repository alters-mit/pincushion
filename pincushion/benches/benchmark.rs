use std::{
    fs::write,
    time::{Duration, Instant},
};

use pincushion::Mesh;

pub fn main() {
    let mesh = Mesh::from_obj("tests/suzanne.obj");
    // Run the benchmark many times and average the result.
    let num_iterations = 1000;
    let dt = (0..num_iterations)
        .map(|_| benchmark(&mesh))
        .sum::<Duration>()
        .as_micros()
        / num_iterations as u128;
    let text = format!("Sampling: {}μs", dt);
    write("../doc/benchmark.txt", &text).unwrap();
    println!("{}", text);
}

fn benchmark(mesh: &Mesh) -> Duration {
    let t0 = Instant::now();
    mesh.sample_points(80., 1.);
    Instant::now() - t0
}
