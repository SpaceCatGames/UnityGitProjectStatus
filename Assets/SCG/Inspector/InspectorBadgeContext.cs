namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Describes the Inspector callback context used to decide whether a badge should be drawn.
    /// It also records whether the active header belongs to an importer-backed asset editor.
    /// </summary>
    internal readonly struct InspectorBadgeContext
    {
        /// <summary>Gets the resolved Unity asset path represented by the current Inspector header.</summary>
        public string AssetPath { get; }

        /// <summary>Gets a value indicating whether the current Inspector header belongs to an importer editor.</summary>
        public bool IsImporterEditor { get; }

        /// <summary>
        /// Initializes a new inspector badge context.
        /// The context keeps the resolved asset path and editor classification together.
        /// This allows the Inspector badge pipeline to stay independent from Project badge rules.
        /// </summary>
        /// <param name="assetPath">Resolved Unity asset path for the inspected header.</param>
        /// <param name="isImporterEditor">Whether the header belongs to an importer editor.</param>
        public InspectorBadgeContext(string assetPath, bool isImporterEditor)
        {
            AssetPath = assetPath;
            IsImporterEditor = isImporterEditor;
        }
    }
}
