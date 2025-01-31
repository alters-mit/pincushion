//! This example uses Macroquad, which is easy to use but not suitable for real-world 3D rendering.

use macroquad::prelude::*;

/// Add feature "obj" to enable `from_obj`.
use pincushion::Mesh as PincushionMesh;

#[macroquad::main("3D")]
async fn main() {
    // Add feature "obj" to enable `from_obj`.
    let pincushion_mesh = PincushionMesh::from_obj("tests/suzanne.obj");
    let macroquad_vertices = pincushion_mesh
        .vertices
        .iter()
        .map(|v| Vertex::new(v.x, v.y, v.z, 0.0, 0.0, PURPLE))
        .collect();
    let macroquad_indices = pincushion_mesh
        .triangles
        .iter()
        .map(|t| [t.a as u16, t.b as u16, t.c as u16])
        .flatten()
        .collect();
    // Create the mesh.
    let mesh = Mesh {
        vertices: macroquad_vertices,
        indices: macroquad_indices,
        texture: None,
    };
    // Sample the points and convert to macroquad Vec3's.
    let points = pincushion_mesh
        .sample_points(30., 1., None)
        .0
        .iter()
        .map(|point| vec3(point.x, point.y, point.z))
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
