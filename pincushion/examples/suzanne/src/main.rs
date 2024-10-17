//! This example uses Macroquad, which is easy to use but not suitable for real-world 3D rendering.

use macroquad::prelude::*;

use tobj::{load_obj, GPU_LOAD_OPTIONS};

use pincushion::{sample_points_from_ppm, Triangle, Vertex as PincushionVertex};

#[macroquad::main("3D")]
async fn main() {
    // Load the obj.
    let obj = &load_obj("tests/suzanne.obj", &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
    let vertices = obj
        .positions
        .chunks_exact(3)
        .map(|v| Vertex::new(v[0], v[1], v[2], 0.0, 0.0, PURPLE))
        .collect();
    let indices = obj.indices.iter().map(|p| *p as u16).collect();
    // Create the mesh.
    let mesh = Mesh {
        vertices,
        indices,
        texture: None,
    };
    // Get pincushion data.
    let vertices = obj
        .positions
        .chunks_exact(3)
        .map(|v| [v[0], v[1], v[2]])
        .collect::<Vec<PincushionVertex>>();
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
    // Sample the points and convert to macroquad Vec3's.
    let points = sample_points_from_ppm(0.15, &vertices, &triangles)
        .iter()
        .map(|point| vec3(point[0], point[1], point[2]))
        .collect::<Vec<Vec3>>();

    loop {
        clear_background(LIGHTGRAY);
        set_camera(&Camera3D {
            position: vec3(-2.5, 2.5, 7.),
            up: vec3(0., 1., 0.),
            target: vec3(0., 0., 0.),
            fovy: 1.5,
            ..Default::default()
        });

        // Draw the mesh.
        draw_mesh(&mesh);

        // Draw the points.
        points.iter().for_each(|point| {
            draw_sphere(*point, 0.02, None, YELLOW);
        });
        next_frame().await
    }
}
