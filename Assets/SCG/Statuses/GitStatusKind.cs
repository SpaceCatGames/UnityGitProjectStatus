namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Represents the normalized Git status kinds used by overlays and the status window.
    /// Values map Git porcelain states to the package status model.
    /// Declaration order does not define badge priority.
    /// </summary>
    public enum GitStatusKind
    {
        /// <summary>No visible status is associated with the asset path.</summary>
        None,

        /// <summary>The tracked item was modified.</summary>
        Modified,

        /// <summary>The tracked item was added.</summary>
        Added,

        /// <summary>The tracked item was deleted.</summary>
        Deleted,

        /// <summary>The tracked item was renamed.</summary>
        Renamed,

        /// <summary>The tracked item was copied.</summary>
        Copied,

        /// <summary>The item is untracked.</summary>
        Untracked,

        /// <summary>The item is in a conflicted merge state.</summary>
        Conflicted,

        /// <summary>The item is ignored by Git.</summary>
        Ignored
    }
}
