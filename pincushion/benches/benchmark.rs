use std::{fs::write, time::Instant};

use tobj::{load_obj, GPU_LOAD_OPTIONS};

use pincushion::{Vertex, Triangle, sample_points_from_ppm, points_to_icosahedrons};

pub fn main() {
    let (vertices, triangles) = get_obj();
    let t0 = Instant::now();
    let points = sample_points_from_ppm(&vertices, &triangles, 0.015);
    let dt_sampling = Instant::now() - t0;
    let t0 = Instant::now();
    points_to_icosahedrons(&points, 0.02);
    let dt_icosahedrons = Instant::now() - t0;
    let text = format!("Sampling: {:?}\nIcosahedrons: {:?}", dt_sampling, dt_icosahedrons);
    write("../benchmark.txt", &text).unwrap();
    println!("{}", text);
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