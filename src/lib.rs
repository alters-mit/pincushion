pub mod vector3;

use vector3::Vector3;

/// Source: https://stackoverflow.com/a/1568551
pub fn signed_volume_of_triangle<T>(p0: &T, p1: &T, p2: &T) -> f32
where
    T: Vector3,
{
    let v321 = p2.x() * p1.y() * p0.z();
    let v231 = p1.x() * p2.y() * p0.z();
    let v312 = p2.x() * p0.y() * p1.z();
    let v132 = p0.x() * p2.y() * p1.z();
    let v213 = p1.x() * p0.y() * p2.z();
    let v123 = p0.x() * p1.y() * p2.z();
    (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123)
}

pub fn get_volume<T>(vertices: &[T], triangles: &[usize]) -> f32
where
    T: Vector3,
{
    triangles
        .chunks_exact(3)
        .map(|triangle| {
            signed_volume_of_triangle(
                &vertices[triangle[0]],
                &vertices[triangle[1]],
                &vertices[triangle[2]],
            )
        })
        .sum()
}
