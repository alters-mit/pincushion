/// Add feature "obj" to enable `from_obj`.
use pincushion::{from_obj, sample_points};

fn main() {
    let (vertices, triangles, normals) = from_obj("tests/suzanne.obj");
    let points_per_m = 0.15;
    let scale = 1.;
    let _ = sample_points(points_per_m, scale, &vertices, &triangles, &normals);
}
