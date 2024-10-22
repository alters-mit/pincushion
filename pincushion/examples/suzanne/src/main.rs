//! This example uses Macroquad, which is easy to use but not suitable for real-world 3D rendering.

use macroquad::prelude::*;

/// Add feature "obj" to enable `from_obj`.
use pincushion::{from_obj, sample_points};

#[macroquad::main("3D")]
async fn main() {
    let (vertices, triangles, normals) = from_obj("tests/suzanne.obj");
    let macroquad_vertices = vertices
        .iter()
        .map(|v| Vertex::new(v[0], v[1], v[2], 0.0, 0.0, PURPLE))
        .collect();
    let macroquad_indices = triangles.iter().flatten().map(|t| *t as u16).collect();
    // Create the mesh.
    let mesh = Mesh {
        vertices: macroquad_vertices,
        indices: macroquad_indices,
        texture: None,
    };
    // Sample the points and convert to macroquad Vec3's.
    let points = sample_points(0.15, 1., &vertices, &triangles, &normals)
        .0
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
