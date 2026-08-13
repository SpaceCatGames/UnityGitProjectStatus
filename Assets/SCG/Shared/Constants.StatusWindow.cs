namespace SCG.UnityGitStatus
{
    internal static partial class Constants
    {
        #region Status Window EditorPrefs

        /// <summary>EditorPrefs key storing the changed-path sorting mode.</summary>
        public const string StatusSortModeKey = "SCG.UnityGitStatus.StatusSortMode";

        #endregion

        #region Status Window Text

        /// <summary>Tooltip for the changed-path sorting popup.</summary>
        public const string SortModeTooltip = "Choose how changed paths are ordered.";

        /// <summary>Label for ascending path sorting.</summary>
        public const string SortByPathAscendingLabel = "Path (A-Z)";

        /// <summary>Label for descending path sorting.</summary>
        public const string SortByPathDescendingLabel = "Path (Z-A)";

        /// <summary>Label for ascending file-name sorting.</summary>
        public const string SortByFileNameAscendingLabel = "File name (A-Z)";

        /// <summary>Label for descending file-name sorting.</summary>
        public const string SortByFileNameDescendingLabel = "File name (Z-A)";

        /// <summary>Label for file-status grouping.</summary>
        public const string SortByFileStatusLabel = "File status";

        #endregion
    }
}
