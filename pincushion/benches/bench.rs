use criterion::{Criterion, criterion_group, criterion_main};
use pincushion::Mesh;

const POINTS_PER_M: f32 = 80.;
const SCALE: f32 = 1.;

fn criterion_benchmark(c: &mut Criterion) {
    let mesh = Mesh::from_obj("tests/suzanne.obj");
    c.bench_function("sample points", |b| {
        b.iter(|| mesh.sample_points(POINTS_PER_M, SCALE, Some(0)))
    });
    c.bench_function("sample points (more points)", |b| {
        b.iter(|| mesh.sample_points(POINTS_PER_M * 3., SCALE, Some(0)))
    });
    c.bench_function("sample triangles", |b| {
        b.iter(|| {
            let area = mesh.get_area(SCALE);
            let _ = mesh.sample_triangles(POINTS_PER_M, &area, Some(0));
        })
    });

    let matrix = vec![
        0.9552104,
        -0.1993398,
        -0.2187162,
        0.,
        0.2042859,
        0.9789113,
        -3.72529E-09,
        0.,
        0.2141038,
        -0.04468064,
        0.9757885,
        0.,
        1.04,
        1.34,
        1.93,
        1.,
    ];

    #[cfg(feature = "ffi")]
    let matrix = matrix.into();
    #[cfg(not(feature = "ffi"))]
    let matrix = glam::Mat4::from_cols_slice(&matrix);

    let mut vertices = mesh.vertices.clone();

    c.bench_function("transform points", |b| {
        b.iter(|| {
            pincushion::transform_points(&matrix, &mut vertices);
        })
    });
}

criterion_group!(benches, criterion_benchmark);
criterion_main!(benches);
