/// Add feature "obj" to enable `from_obj`.
use pincushion::{from_obj, sample_points_from_ppm};

fn main() {
    let (vertices, triangles, normals) = from_obj("tests/suzanne.obj");
    let points_per_m = 0.15;
    let _ = sample_points_from_ppm(points_per_m, &vertices, &triangles, &normals);
}
