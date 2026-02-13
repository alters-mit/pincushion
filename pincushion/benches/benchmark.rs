use std::{
    fs::write,
    time::{Duration, Instant},
};

use pincushion::{Mesh, transform_points};

pub fn main() {
    let mesh = Mesh::from_obj("tests/suzanne.obj");
    // Run the benchmark many times and average the result.
    let num_iterations = 1000;
    let dts = (0..num_iterations)
        .map(|_| benchmark(&mesh))
        .collect::<Vec<(Duration, Duration, Duration)>>();
    let dt_sample =
        dts.iter().map(|dt| dt.0).sum::<Duration>().as_micros() / num_iterations as u128;
    let dt_triangles =
        dts.iter().map(|dt| dt.1).sum::<Duration>().as_micros() / num_iterations as u128;
    let dt_transform = dts.iter().map(|dt| dt.2).sum::<Duration>().as_nanos() as f32
        / 1000.
        / num_iterations as f32;
    let text = format!(
        "Sample points: {}μs\n\nSample triangles: {}μs\n\nTransformed points: {}μs",
        dt_sample, dt_triangles, dt_transform
    );
    write("../doc/benchmark.txt", &text).unwrap();
    println!("{}", text);
}

fn benchmark(mesh: &Mesh) -> (Duration, Duration, Duration) {
    const POINTS_PER_M: f32 = 80.;
    const SCALE: f32 = 1.;

    let t0 = Instant::now();
    let _ = mesh.sample_points(POINTS_PER_M, SCALE, Some(0));
    let dt_sample = Instant::now() - t0;

    let t0 = Instant::now();
    let area = mesh.get_area(SCALE);
    let _ = mesh.sample_triangles(POINTS_PER_M, &area, Some(0));
    let dt_triangles = Instant::now() - t0;

    let matrix = vec![
        0.9552104,
        -0.1993398,
        -0.2187162,
        0.,
        0.2042859,
        0.9789113,
        -3.72529E-09,
        0.,
        0.2141038,
        -0.04468064,
        0.9757885,
        0.,
        1.04,
        1.34,
        1.93,
        1.,
    ]
    .into();
    let mut vertices = mesh.vertices.clone();
    let t0 = Instant::now();
    transform_points(&matrix, &mut vertices);
    let dt_transform = Instant::now() - t0;

    (dt_sample, dt_triangles, dt_transform)
}
