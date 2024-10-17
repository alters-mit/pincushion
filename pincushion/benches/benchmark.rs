use std::{
    fs::write,
    time::{Duration, Instant},
};

use tobj::{load_obj, GPU_LOAD_OPTIONS};

use pincushion::{points_to_quads, sample_points_from_ppm, Triangle, Uv, Vertex};

pub fn main() {
    let (vertices, triangles) = get_obj();
    // Run the benchmark many times.
    let dts = (0..1000)
        .map(|_| benchmark(&vertices, &triangles))
        .collect::<Vec<(Duration, Duration)>>();
    // Average the results.
    let dt_sampling = dts.iter().map(|(s, _)| s).sum::<Duration>().as_micros() / dts.len() as u128;
    let dt_quads = dts.iter().map(|(_, i)| i).sum::<Duration>().as_micros() / dts.len() as u128;
    let text = format!("Sampling: {}μs\nQuads: {}μs", dt_sampling, dt_quads);
    write("../benchmark.txt", &text).unwrap();
    println!("{}", text);
}

fn benchmark(vertices: &[Vertex], triangles: &[Triangle]) -> (Duration, Duration) {
    let t0 = Instant::now();
    let points = sample_points_from_ppm(0.15, &vertices, &triangles);
    let dt_sampling = Instant::now() - t0;
    let t0 = Instant::now();
    points_to_quads::<_, Triangle, Uv>(0.02, &points);
    let dt_quads = Instant::now() - t0;
    (dt_sampling, dt_quads)
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
