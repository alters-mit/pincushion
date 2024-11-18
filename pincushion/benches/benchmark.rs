use std::{
    fs::write,
    time::{Duration, Instant},
};

use pincushion::Mesh;

pub fn main() {
    let mesh = Mesh::from_obj("tests/suzanne.obj");
    // Run the benchmark many times and average the result.
    let num_iterations = 1000;
    let dts = (0..num_iterations)
        .map(|_| benchmark(&mesh))
        .collect::<Vec<(Duration, Duration)>>();
    let dt_sample =
        dts.iter().map(|dt| dt.0).sum::<Duration>().as_micros() / num_iterations as u128;
    let dt_triangles =
        dts.iter().map(|dt| dt.1).sum::<Duration>().as_micros() / num_iterations as u128;
    let text = format!(
        "Sample points: {}μs\n\nSample triangles: {}μs",
        dt_sample, dt_triangles
    );
    write("../doc/benchmark.txt", &text).unwrap();
    println!("{}", text);
}

fn benchmark(mesh: &Mesh) -> (Duration, Duration) {
    const POINTS_PER_M: f32 = 80.;
    const SCALE: f32 = 1.;

    let t0 = Instant::now();
    let _ = mesh.sample_points(POINTS_PER_M, SCALE);
    let dt_sample = Instant::now() - t0;

    let t0 = Instant::now();
    let area = mesh.get_area(SCALE);
    let _ = mesh.sample_triangles(POINTS_PER_M, &area);
    let dt_triangles = Instant::now() - t0;

    (dt_sample, dt_triangles)
}
