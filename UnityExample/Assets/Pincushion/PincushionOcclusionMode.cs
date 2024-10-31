namespace Pincushion
{
    /// <summary>
    /// How occlusion is handled when rendering a pincushion sampled mesh.
    /// </summary>
    public enum PincushionOcclusionMode
    {
        /// <summary>
        /// No occlusion other than points occluding points.
        /// Don't hide the source meshes.
        /// </summary>
        None,
        /// <summary>
        /// No occlusion other than points occluding points.
        /// Hide the source meshes.
        /// </summary>
        HideSourceMeshes,
        /// <summary>
        /// Don't draw points facing away from the camera.
        /// Hide the source meshes.
        /// </summary>
        Backfacing,
        /// <summary>
        /// Don't draw points facing away from the camera.
        /// Source meshes will become solid colors and occlude points.
        /// </summary>
        Behind
    }
}