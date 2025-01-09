namespace Pincushion
{
    /// <summary>
    /// How Pincushion renders sampled meshes.
    /// </summary>
    public enum PincushionRenderMode
    {
        /// <summary>
        /// Hide the sampled meshes.
        /// Show the source meshes.
        /// Show the original camera background.
        /// </summary>
        DoNot,
        /// <summary>
        /// Show the sampled meshes.
        /// Show the source meshes.
        /// </summary>
        WithSourceMeshes,
        /// <summary>
        /// Show the sampled meshes.
        /// Hide the source meshes.
        /// </summary>
        WithoutSourceMeshes,
        /// <summary>
        /// Hide sampled points facing away from the camera.
        /// Hide the source meshes.
        /// </summary>
        HideBackfacing,
        /// <summary>
        /// Hide sampled points that would be behind source meshes.
        /// Hide the source meshes.
        /// </summary>
        OccludeBehind
    }
}