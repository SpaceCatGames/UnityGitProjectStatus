using UnityEditor;
using UnityEditorInternal;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Exposes the package editor settings backed by Unity EditorPrefs.
    /// These settings control refresh behavior, badge appearance, and Project or Inspector integrations. 
    /// </summary>
    public static class UnityGitStatusSettings
    {
        #region Properties

        /// <summary>Gets or sets the refresh mode used by the shared Git status cache.</summary>
        public static GitRefreshMode RefreshMode
        {
            get
            {
                var storedValue = GetIntWithLegacyKey(
                    Constants.RefreshModeKey,
                    Constants.LegacyRefreshModeKey,
                    (int)GitRefreshMode.EventDriven);
                return System.Enum.IsDefined(typeof(GitRefreshMode), storedValue)
                    ? (GitRefreshMode)storedValue
                    : GitRefreshMode.EventDriven;
            }
            set => EditorPrefs.SetInt(Constants.RefreshModeKey, (int)value);
        }

        /// <summary>Gets or sets the timed refresh interval in seconds after clamping to the supported range.</summary>
        public static int TimedRefreshIntervalSeconds
        {
            get
            {
                var storedValue = GetIntWithLegacyKey(
                    Constants.TimedRefreshIntervalSecondsKey,
                    Constants.LegacyTimedRefreshIntervalSecondsKey,
                    Constants.DefaultTimedRefreshIntervalSeconds);
                return ClampTimedRefreshInterval(storedValue);
            }
            set => EditorPrefs.SetInt(
                Constants.TimedRefreshIntervalSecondsKey,
                ClampTimedRefreshInterval(value));
        }

        /// <summary>Gets or sets whether Project window badges are drawn.</summary>
        public static bool ProjectOverlaysEnabled
        {
            get => GetBoolWithLegacyKey(
                Constants.ProjectOverlaysEnabledKey,
                Constants.LegacyProjectOverlaysEnabledKey,
                true);
            set
            {
                EditorPrefs.SetBool(Constants.ProjectOverlaysEnabledKey, value);
                RepaintBadgeViews();
            }
        }

        /// <summary>Gets or sets whether Unity meta files are shown in the status window list.</summary>
        public static bool ShowMetaFiles
        {
            get => GetBoolWithLegacyKey(Constants.ShowMetaFilesKey, Constants.LegacyShowMetaFilesKey, false);
            set => EditorPrefs.SetBool(Constants.ShowMetaFilesKey, value);
        }

        /// <summary>Gets or sets the number of unchanged context lines shown around diff changes.</summary>
        public static int DiffContextLines
        {
            get => ClampDiffContextLines(EditorPrefs.GetInt(
                Constants.DiffContextLinesKey,
                Constants.DefaultDiffContextLines));
            set => EditorPrefs.SetInt(Constants.DiffContextLinesKey, ClampDiffContextLines(value));
        }

        /// <summary>Gets or sets how changed paths are ordered in the Git Status window.</summary>
        internal static GitStatusSortMode StatusSortMode
        {
            get
            {
                var storedValue = EditorPrefs.GetInt(
                    Constants.StatusSortModeKey,
                    (int)GitStatusSortMode.PathAscending);
                return System.Enum.IsDefined(typeof(GitStatusSortMode), storedValue)
                    ? (GitStatusSortMode)storedValue
                    : GitStatusSortMode.PathAscending;
            }
            set => EditorPrefs.SetInt(Constants.StatusSortModeKey, (int)value);
        }

        /// <summary>Gets or sets whether the deleted-files footer is shown in the Project window.</summary>
        public static bool ShowDeletedFilesInProject
        {
            get => GetBoolWithLegacyKey(
                Constants.ShowDeletedFilesInProjectKey,
                Constants.LegacyShowDeletedFilesInProjectKey,
                true);
            set
            {
                EditorPrefs.SetBool(Constants.ShowDeletedFilesInProjectKey, value);
                EditorApplication.RepaintProjectWindow();
            }
        }

        /// <summary>Gets or sets whether Project window badges are drawn at the right edge of each row.</summary>
        public static bool RightAlignedBadges
        {
            get => GetBoolWithLegacyKey(
                Constants.RightAlignedBadgesKey,
                Constants.LegacyRightAlignedBadgesKey,
                true);
            set
            {
                EditorPrefs.SetBool(Constants.RightAlignedBadgesKey, value);
                RepaintBadgeViews();
            }
        }

        /// <summary>Gets or sets whether the Inspector header badge is drawn for the primary inspected asset.</summary>
        public static bool InspectorBadgeEnabled
        {
            get => GetBoolWithLegacyKeys(
                Constants.InspectorBadgeEnabledKey,
                Constants.LegacyInspectorBadgeEnabledKey,
                Constants.LegacyShowInspectorBadgeKey,
                true);
            set
            {
                EditorPrefs.SetBool(Constants.InspectorBadgeEnabledKey, value);
                RepaintBadgeViews();
            }
        }

        /// <summary>Gets or sets whether status badges use symbolic Calc Mode markers instead of letter markers.</summary>
        public static bool CalcMode
        {
            get => GetBoolWithLegacyKey(Constants.CalcModeKey, Constants.LegacyCalcModeKey, false);
            set
            {
                EditorPrefs.SetBool(Constants.CalcModeKey, value);
                RepaintBadgeViews();
            }
        }

        /// <summary>Gets or sets whether badges are drawn in the left tree pane of the two-column Project layout.</summary>
        public static bool ShowLeftPaneOverlaysInTwoColumn
        {
            get => GetBoolWithLegacyKey(
                Constants.ShowLeftPaneOverlaysInTwoColumnKey,
                Constants.LegacyShowLeftPaneOverlaysInTwoColumnKey,
                true);
            set
            {
                EditorPrefs.SetBool(Constants.ShowLeftPaneOverlaysInTwoColumnKey, value);
                RepaintBadgeViews();
            }
        }

        /// <summary>Gets or sets whether Project-window deleted entries stay expanded for the current user.</summary>
        public static bool ShowProjectDeletedEntries
        {
            get => GetBoolWithLegacyKey(
                Constants.ProjectDeletedEntriesExpandedKey,
                Constants.LegacyProjectDeletedEntriesExpandedKey,
                true);
            set => EditorPrefs.SetBool(Constants.ProjectDeletedEntriesExpandedKey, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Applies a new refresh mode to the shared cache settings.
        /// Reapplying the current mode is ignored to avoid unnecessary refresh work.
        /// Automatic modes can trigger follow-up refresh scheduling inside the cache.
        /// </summary>
        /// <param name="refreshMode">Refresh mode that should become active.</param>
        public static void ApplyRefreshMode(GitRefreshMode refreshMode)
        {
            if (RefreshMode == refreshMode)
            {
                return;
            }

            RefreshMode = refreshMode;
            GitStatusCache.ApplyRefreshSettingsChange(refreshMode != GitRefreshMode.ManualOnly);
        }

        /// <summary>
        /// Repaints editor views that can display Git status UI.
        /// This is used after settings changes that affect badge visibility or appearance.
        /// It repaints both Project-related views and broader editor windows.
        /// </summary>
        public static void RepaintBadgeViews()
        {
            EditorApplication.RepaintProjectWindow();
            InternalEditorUtility.RepaintAllViews();
        }

        private static bool GetBoolWithLegacyKey(string key, string legacyKey, bool defaultValue)
        {
            if (EditorPrefs.HasKey(key)) return EditorPrefs.GetBool(key, defaultValue);
            if (!EditorPrefs.HasKey(legacyKey)) return defaultValue;

            var value = EditorPrefs.GetBool(legacyKey, defaultValue);
            EditorPrefs.SetBool(key, value);
            return value;
        }

        private static bool GetBoolWithLegacyKeys(
            string key,
            string legacyKey,
            string olderLegacyKey,
            bool defaultValue)
        {
            if (EditorPrefs.HasKey(key)) return EditorPrefs.GetBool(key, defaultValue);
            if (EditorPrefs.HasKey(legacyKey)) return MigrateBool(key, legacyKey, defaultValue);
            return EditorPrefs.HasKey(olderLegacyKey)
                ? MigrateBool(key, olderLegacyKey, defaultValue)
                : defaultValue;
        }

        private static bool MigrateBool(string key, string legacyKey, bool defaultValue)
        {
            var value = EditorPrefs.GetBool(legacyKey, defaultValue);
            EditorPrefs.SetBool(key, value);
            return value;
        }

        private static int GetIntWithLegacyKey(string key, string legacyKey, int defaultValue)
        {
            if (EditorPrefs.HasKey(key)) return EditorPrefs.GetInt(key, defaultValue);
            if (!EditorPrefs.HasKey(legacyKey)) return defaultValue;

            var value = EditorPrefs.GetInt(legacyKey, defaultValue);
            EditorPrefs.SetInt(key, value);
            return value;
        }

        private static int ClampTimedRefreshInterval(int intervalSeconds) =>
            intervalSeconds < Constants.MinTimedRefreshIntervalSeconds
                ? Constants.MinTimedRefreshIntervalSeconds
                : intervalSeconds > Constants.MaxTimedRefreshIntervalSeconds
                    ? Constants.MaxTimedRefreshIntervalSeconds
                    : intervalSeconds;

        private static int ClampDiffContextLines(int contextLines) =>
            contextLines < Constants.MinDiffContextLines
                ? Constants.MinDiffContextLines
                : contextLines > Constants.MaxDiffContextLines
                    ? Constants.MaxDiffContextLines
                    : contextLines;

        #endregion
    }
}
