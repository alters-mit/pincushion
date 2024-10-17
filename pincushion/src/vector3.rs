pub trait Vector3 {
    fn new(x: f32, y: f32, z: f32) -> Self;
    fn x(&self) -> f32;
    fn y(&self) -> f32;
    fn z(&self) -> f32;
    fn x_mut(&mut self) -> &mut f32;
    fn y_mut(&mut self) -> &mut f32;
    fn z_mut(&mut self) -> &mut f32;

    // For add, sub, etc. see: glam::Vec3

    fn add(&self, other: &Self) -> Self
    where
        Self: Sized,
    {
        Self::new(
            self.x() + other.x(),
            self.y() + other.y(),
            self.z() + other.z(),
        )
    }

    fn add_mut(&mut self, other: &Self)
    where
        Self: Sized,
    {
        *self.x_mut() += other.x();
        *self.y_mut() += other.y();
        *self.z_mut() += other.z();
    }

    fn sub(&self, other: &Self) -> Self
    where
        Self: Sized,
    {
        Self::new(
            self.x() * other.x(),
            self.y() * other.y(),
            self.z() * other.z(),
        )
    }

    fn mul(&self, other: f32) -> Self
    where
        Self: Sized,
    {
        Self::new(
            self.x() * other,
            self.y() * other,
            self.z() * other,
        )
    }

    fn mul_mut(&mut self, other: f32)
    where
        Self: Sized,
    {
        *self.x_mut() *= other;
        *self.y_mut() *= other;
        *self.z_mut() *= other;
    }

    fn cross(&self, other: &Self) -> Self
    where
        Self: Sized,
    {
        Self::new(
            self.y() * other.z() - other.y() * self.z(),
            self.z() * other.x() - other.z() * self.x(),
            self.x() * other.y() - other.x() * self.y(),
        )
    }

    fn dot(&self, other: &Self) -> f32 {
        (self.x() * other.x()) + (self.y() * other.y()) + (self.z() * other.z())
    }

    fn magnitude(&self) -> f32 {
        f32::sqrt(self.dot(self))
    }

    /// Returns the area of a triangle.
    /// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L202
    fn get_triangle_area(p0: &Self, p1: &Self, p2: &Self) -> f32
    where
        Self: Sized
    {
        0.5 * &p1.sub(p0).cross(&p2.sub(p0)).magnitude()
    }
}

impl Vector3 for [f32; 3] {
    fn new(x: f32, y: f32, z: f32) -> Self {
        [x, y, z]
    }

    fn x(&self) -> f32 {
        self[0]
    }

    fn y(&self) -> f32 {
        self[1]
    }

    fn z(&self) -> f32 {
        self[2]
    }

    fn x_mut(&mut self) -> &mut f32 {
        &mut self[0]
    }

    fn y_mut(&mut self) -> &mut f32 {
        &mut self[1]
    }

    fn z_mut(&mut self) -> &mut f32 {
        &mut self[2]
    }
}

pub trait Vector3U {
    fn new(x: usize, y: usize, z: usize) -> Self;
    fn x(&self) -> usize;
    fn y(&self) -> usize;
    fn z(&self) -> usize;
    fn x_mut(&mut self) -> &mut usize;
    fn y_mut(&mut self) -> &mut usize;
    fn z_mut(&mut self) -> &mut usize;
}

impl Vector3U for [usize; 3] {
    fn new(x: usize, y: usize, z: usize) -> Self {
        [x, y, z]
    }

    fn x(&self) -> usize {
        self[0]
    }

    fn y(&self) -> usize {
        self[1]
    }

    fn z(&self) -> usize {
        self[2]
    }

    fn x_mut(&mut self) -> &mut usize {
        &mut self[0]
    }

    fn y_mut(&mut self) -> &mut usize {
        &mut self[1]
    }

    fn z_mut(&mut self) -> &mut usize {
        &mut self[2]
    }
}