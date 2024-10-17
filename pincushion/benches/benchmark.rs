use std::{
    fs::write,
    time::{Duration, Instant},
};

use tobj::{load_obj, GPU_LOAD_OPTIONS};

use pincushion::{points_to_icosahedrons, sample_points_from_ppm, Triangle, Vertex};

pub fn main() {
    let (vertices, triangles) = get_obj();
    // Run the benchmark many times.
    let dts = (0..1000)
        .map(|_| benchmark(&vertices, &triangles))
        .collect::<Vec<(Duration, Duration)>>();
    // Average the results.
    let dt_sampling = dts.iter().map(|(s, _)| s).sum::<Duration>().as_micros() / dts.len() as u128;
    let dt_icosahedrons =
        dts.iter().map(|(_, i)| i).sum::<Duration>().as_micros() / dts.len() as u128;
    let text = format!(
        "Sampling: {}μs\nIcosahedrons: {}μs",
        dt_sampling, dt_icosahedrons
    );
    write("../benchmark.txt", &text).unwrap();
    println!("{}", text);
}

fn benchmark(vertices: &[Vertex], triangles: &[Triangle]) -> (Duration, Duration) {
    let t0 = Instant::now();
    let points = sample_points_from_ppm(&vertices, &triangles, 0.015, 1.);
    let dt_sampling = Instant::now() - t0;
    let t0 = Instant::now();
    points_to_icosahedrons(&points, 0.02);
    let dt_icosahedrons = Instant::now() - t0;
    (dt_sampling, dt_icosahedrons)
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
