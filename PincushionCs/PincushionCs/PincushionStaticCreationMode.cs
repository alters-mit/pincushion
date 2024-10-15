namespace Pincushion
{
    /// <summary>
    /// How static sample points will be added to the scene.
    /// </summary>
    public enum PincushionStaticCreationMode
    {
        /// <summary>
        /// Create a new GameObject and mesh with sampled points.
        /// </summary>
        create,
        /// <summary>
        /// Create a new GameObject and mesh with sampled points.
        /// Keep the original GameObject but hide it.
        /// </summary>
        createAndHideOriginal,
        /// <summary>
        /// Replace the original mesh with the sampled points mesh.
        /// No new GameObject is created.
        /// </summary>
        replace
    }
}