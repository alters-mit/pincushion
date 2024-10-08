use std::{fs::File, io::BufReader};

use macroquad::prelude::*;

use obj::raw::{object::Polygon, parse_obj};

use pincushion::sample_points_from_ppcm;

#[macroquad::main("3D")]
async fn main() {
    // Load the obj.
    let input = BufReader::new(File::open("tests/suzanne.obj").unwrap());
    let obj = parse_obj(input).unwrap();
    let vertices = obj.positions.iter().map(|v| Vertex::new(v.0, v.1, v.2, 0.0, 0.0, PURPLE)).collect();
    let indices = obj.polygons.iter().map(|p| match p {
        Polygon::PN(pn) => [pn[0].0 as u16, pn[1].0 as u16, pn[2].0 as u16],
        _ => unreachable!()
    }).flatten().collect();
    // Create the mesh.
    let mesh = Mesh {
        vertices,
        indices,
        texture: None
    };
    // Get pincushion data.
    let vertices = obj.positions.iter().map(|v| [v.0, v.1, v.2]).collect::<Vec<[f32; 3]>>();
    let triangles = obj.polygons.iter().map(|p| match p {
        Polygon::P(triangle) => [triangle[0], triangle[1], triangle[2]],
        Polygon::PN(pn) => [pn[0].0, pn[1].0, pn[2].0],
        _ => unreachable!()
    }).collect::<Vec<[usize; 3]>>();
    // Sample the points and convert to macroquad Vec3's.
    let points = sample_points_from_ppcm(&vertices, &triangles, 1.5).iter().map(|point| vec3(point[0], point[1], point[2])).collect::<Vec<Vec3>>();

    loop {
        clear_background(LIGHTGRAY);
        set_camera(&Camera3D {
            position: vec3(1., -1., -2.),
            up: vec3(0., 1., 0.),
            target: vec3(0., 0., 0.),
            ..Default::default()
        });

        // Draw the mesh.
        draw_mesh(&mesh);

        // Draw the points.
        points.iter().for_each(|point| {
            draw_sphere(*point, 0.05, None, YELLOW);
        });
        next_frame().await
    }
}