use pincushion::Mesh;

fn main() {
    // Add feature "obj" to enable `from_obj`.
    let mesh = Mesh::from_obj("tests/suzanne.obj");
    let points_per_m = 80.;
    let scale = 1.;
    let seed = Some(0);
    let _ = mesh.sample_points(points_per_m, scale, seed);
}
