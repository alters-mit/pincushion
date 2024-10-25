namespace Pincushion
{
    /// <summary>
    /// How occlusion is handled when rendering a pincushion sampled mesh.
    /// </summary>
    public enum PincushionOcclusionMode
    {
        /// <summary>
        /// No occlusion other than points occluding points.
        /// </summary>
        None,
        /// <summary>
        /// Don't draw points facing away from the camera.
        /// </summary>
        Backfacing,
        /// <summary>
        /// Don't draw points facing away from the camera.
        /// Source meshes will become solid colors and occlude points.
        /// </summary>
        SourceMesh
    }
}