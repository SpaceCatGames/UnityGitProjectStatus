namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Defines constants used by menus, settings, Git execution, project paths, and layout calculations.
    /// Centralizing these values keeps package behavior consistent across the editor code.
    /// The class also avoids repeating literals in related systems.
    /// </summary>
    internal static partial class Constants
    {
        #region Menu Paths

        /// <summary>Root menu path for package actions under the menu.</summary>
        public const string MenuRoot = "SCG/Git Project Status/";

        /// <summary>Menu path used to trigger an immediate refresh.</summary>
        public const string RefreshMenuPath = MenuRoot + "Refresh";

        /// <summary>Root menu path used for all badge-related settings.</summary>
        public const string BadgeSettingsMenuRoot = MenuRoot + "Badge Settings/";

        /// <summary>Root menu path used for badge appearance settings shared across Project and Inspector.</summary>
        public const string BadgeAppearanceMenuRoot = BadgeSettingsMenuRoot + "Appearance/";

        /// <summary>Root menu path used for Project window badge settings.</summary>
        public const string BadgeProjectMenuRoot = BadgeSettingsMenuRoot + "Project/";

        /// <summary>Root menu path used for Inspector badge settings.</summary>
        public const string BadgeInspectorMenuRoot = BadgeSettingsMenuRoot + "Inspector/";

        /// <summary>Menu path used to toggle deleted files footer visibility in the Project window.</summary>
        public const string ShowDeletedFilesInProjectMenuPath = BadgeProjectMenuRoot + "Deleted Files in Project";

        /// <summary>Menu path used to toggle left pane overlays in the Project window two-column layout.</summary>
        public const string ShowLeftPaneOverlaysInTwoColumnMenuPath = BadgeProjectMenuRoot + "Left Pane Overlays in Two Column";

        /// <summary>Menu path used to toggle right-aligned badge positioning in the Project window.</summary>
        public const string RightAlignedBadgesMenuPath = BadgeProjectMenuRoot + "Right-Aligned Badges";

        /// <summary>Menu path used to toggle Calc Mode for badge symbols.</summary>
        public const string CalcModeMenuPath = BadgeAppearanceMenuRoot + "Calc Mode";

        /// <summary>Menu path used to toggle Project window overlays.</summary>
        public const string ProjectOverlaysMenuPath = BadgeProjectMenuRoot + "Enable Project Overlays";

        /// <summary>Menu path used to toggle the Inspector badge for the inspected asset or folder.</summary>
        public const string InspectorBadgeMenuPath = BadgeInspectorMenuRoot + "Enable Inspector Badge";

        /// <summary>Root menu path used for refresh mode selection.</summary>
        public const string RefreshModeMenuRoot = MenuRoot + "Refresh Mode/";

        /// <summary>Menu path used to switch the package to manual-only refresh.</summary>
        public const string RefreshModeManualMenuPath = RefreshModeMenuRoot + "Manual Only";

        /// <summary>Menu path used to switch the package to timed refresh.</summary>
        public const string RefreshModeTimedMenuPath = RefreshModeMenuRoot + "Timed";

        /// <summary>Menu path used to switch the package to event-driven refresh.</summary>
        public const string RefreshModeEventDrivenMenuPath = RefreshModeMenuRoot + "Event-Driven";

        /// <summary>Window menu path used to open the main status window.</summary>
        public const string WindowMenuPath = MenuRoot + "Project Status Window &g";

        #endregion

        #region Display

        /// <summary>Title shown in the Unity Editor window and related UI.</summary>
        public const string WindowTitle = "Git Status";

        #endregion

        #region EditorPrefs

        /// <summary>EditorPrefs key storing whether Project window overlays are enabled.</summary>
        public const string ProjectOverlaysEnabledKey = "SCG.GitProjectStatus.OverlayEnabled";

        /// <summary>EditorPrefs key storing whether meta files are visible in the status window list.</summary>
        public const string ShowMetaFilesKey = "SCG.GitProjectStatus.ShowMetaFiles";

        /// <summary>EditorPrefs key storing whether deleted files are shown in the Project footer.</summary>
        public const string ShowDeletedFilesInProjectKey = "SCG.GitProjectStatus.ShowDeletedFilesInProject";

        /// <summary>EditorPrefs key storing whether overlays are shown in the left pane for two-column Project layout.</summary>
        public const string ShowLeftPaneOverlaysInTwoColumnKey = "SCG.GitProjectStatus.ShowLeftPaneOverlaysInTwoColumn";

        /// <summary>EditorPrefs key storing whether badges are drawn at the right side of Project rows.</summary>
        public const string RightAlignedBadgesKey = "SCG.GitProjectStatus.RightAlignedBadges";

        /// <summary>EditorPrefs key storing whether the Inspector header badge is enabled.</summary>
        public const string InspectorBadgeEnabledKey = "SCG.GitProjectStatus.InspectorBadgeEnabled";

        /// <summary>Legacy EditorPrefs key used by older builds for the Inspector badge toggle.</summary>
        public const string LegacyShowInspectorBadgeKey = "SCG.GitProjectStatus.ShowInspectorBadge";

        /// <summary>EditorPrefs key storing whether Calc Mode is enabled for badge symbols.</summary>
        public const string CalcModeKey = "SCG.GitProjectStatus.CalcMode";

        /// <summary>EditorPrefs key storing whether the Project deleted files block is expanded.</summary>
        public const string ProjectDeletedEntriesExpandedKey = "SCG.GitProjectStatus.ProjectDeletedEntriesExpanded";

        /// <summary>EditorPrefs key storing the selected refresh mode.</summary>
        public const string RefreshModeKey = "SCG.GitProjectStatus.RefreshMode";

        /// <summary>EditorPrefs key storing the selected timed refresh interval in seconds.</summary>
        public const string TimedRefreshIntervalSecondsKey = "SCG.GitProjectStatus.TimedRefreshIntervalSeconds";

        #endregion

        #region Git

        /// <summary>Name of the Git executable expected on the system PATH.</summary>
        public const string GitExecutableName = "git";

        /// <summary>Default timeout used for Git commands that gather status information.</summary>
        public const int GitCommandTimeoutMilliseconds = 5000;

        /// <summary>Shorter timeout used when resolving the current branch name.</summary>
        public const int BranchCommandTimeoutMilliseconds = 2000;

        /// <summary>Default refresh interval used by timed refresh mode.</summary>
        public const int DefaultTimedRefreshIntervalSeconds = 5;

        /// <summary>Minimum refresh interval allowed by timed refresh mode.</summary>
        public const int MinTimedRefreshIntervalSeconds = 1;

        /// <summary>Maximum refresh interval allowed by timed refresh mode.</summary>
        public const int MaxTimedRefreshIntervalSeconds = 30;

        #endregion

        #region Project Paths

        /// <summary>Unity asset root path used for filtering visible package entries.</summary>
        public const string AssetsRoot = "Assets";

        /// <summary>Unity asset root path with a trailing slash for prefix checks.</summary>
        public const string AssetsRootWithSeparator = AssetsRoot + "/";

        /// <summary>Unity packages root path used for excluding the top-level Packages node from badges.</summary>
        public const string PackagesRoot = "Packages";

        #endregion

        #region Layout

        /// <summary>Square size target used for overlay badges drawn in the Project window.</summary>
        public const float OverlayBadgeSize = 16f;

        /// <summary>Minimum size used when deriving the current right-aligned badge size from the row height.</summary>
        public const float OverlayBadgeMinSize = 10f;

        /// <summary>Right inset used by the current right-aligned badge placement.</summary>
        public const float OverlayRightAlignedBadgeRightInset = 2f;

        /// <summary>Right inset used by the one-column Project layout to match the perceived position from two-column rows.</summary>
        public const float OverlayOneColumnRightAlignedBadgeRightInset = 4f;

        /// <summary>Top inset used by the current right-aligned badge placement.</summary>
        public const float OverlayRightAlignedBadgeTopInset = 1f;

        /// <summary>Left inset used when approximating the icon area for icon-corner badge placement.</summary>
        public const float OverlayIconCornerIconLeftInset = 1f;

        /// <summary>Additional top inset used when approximating the icon area for icon-corner badge placement.</summary>
        public const float OverlayIconCornerIconTopInset = 0f;

        /// <summary>Minimum icon size assumed by the icon-corner badge placement heuristic.</summary>
        public const float OverlayIconCornerIconMinSize = 14f;

        /// <summary>Maximum icon size assumed by the icon-corner badge placement heuristic.</summary>
        public const float OverlayIconCornerIconMaxSize = 16f;

        /// <summary>Multiplier used to derive the compact icon-corner badge size from the icon size.</summary>
        public const float OverlayIconCornerBadgeSizeMultiplier = 0.58f;

        /// <summary>Minimum compact badge size used by the icon-corner placement mode.</summary>
        public const float OverlayIconCornerBadgeMinSize = 8f;

        /// <summary>Maximum compact badge size used by the icon-corner placement mode.</summary>
        public const float OverlayIconCornerBadgeMaxSize = 10f;

        /// <summary>Horizontal offset applied after anchoring the icon-corner badge to the icon top-right corner.</summary>
        public const float OverlayIconCornerBadgeOffsetX = 2f;

        /// <summary>Vertical offset applied after anchoring the icon-corner badge to the icon top-right corner.</summary>
        public const float OverlayIconCornerBadgeOffsetY = -1f;

        /// <summary>Horizontal content offset used to visually center the glyph inside the compact icon-corner badge.</summary>
        public const float OverlayIconCornerBadgeContentOffsetX = 1f;

        /// <summary>Vertical content offset used to visually center the glyph inside the compact icon-corner badge.</summary>
        public const float OverlayIconCornerBadgeContentOffsetY = -0.5f;

        /// <summary>Editor style name used by Unity for the large Inspector header.</summary>
        public const string InspectorHeaderStyleName = "IN BigTitle";

        /// <summary>Editor style name used by Unity for the post-header background group in finishedDefaultHeaderGUI.</summary>
        public const string InspectorPostHeaderStyleName = "IN BigTitle Post";

        /// <summary>Top and left content inset used by Unity DrawHeaderGUI for the large Inspector header icon.</summary>
        public const float InspectorHeaderContentInset = 6f;

        /// <summary>Fallback image section width mirrored from Unity Editor.DrawHeaderGUI.</summary>
        public const float InspectorHeaderImageSectionWidthFallback = 44f;

        /// <summary>Fallback title height mirrored from Unity Editor.DrawHeaderGUI.</summary>
        public const float InspectorHeaderTitleHeightFallback = 21f;

        /// <summary>Icon size used by Unity DrawHeaderGUI for the large Inspector header icon.</summary>
        public const float InspectorHeaderIconSize = 32f;

        /// <summary>Multiplier used to derive the compact Inspector header badge size from the icon size.</summary>
        public const float InspectorHeaderBadgeSizeMultiplier = 0.4f;

        /// <summary>Minimum Inspector header badge size.</summary>
        public const float InspectorHeaderBadgeMinSize = 10f;

        /// <summary>Maximum Inspector header badge size.</summary>
        public const float InspectorHeaderBadgeMaxSize = 12f;

        /// <summary>Overlap compensation used by Unity before entering the post-header background group.</summary>
        public const float InspectorHeaderBottomOverlap = 1f;

        /// <summary>Horizontal offset applied after anchoring the Inspector badge to the icon top-right corner.</summary>
        public const float InspectorHeaderBadgeOffsetX = -1f;

        /// <summary>Vertical offset applied after anchoring the Inspector badge to the icon top-right corner.</summary>
        public const float InspectorHeaderBadgeOffsetY = -13.5f;

        /// <summary>Horizontal gap between the status badge and the path label in the package window list.</summary>
        public const float WindowBadgeHorizontalSpacing = 6f;

        /// <summary>Vertical inset applied when drawing the status badge in the package window list.</summary>
        public const float WindowBadgeVerticalInset = 2f;

        /// <summary>Minimum width used by the Git status editor window.</summary>
        public const float WindowMinWidth = 510f;

        /// <summary>Minimum height used by the Git status editor window.</summary>
        public const float WindowMinHeight = 180f;

        /// <summary>Vertical gap between the changed-paths list and the footer row in the Git status window.</summary>
        public const float WindowFooterTopSpacing = -1.5f;

        /// <summary>Horizontal padding used by the deleted files footer in the Project window.</summary>
        public const float ProjectDeletedFooterPadding = 6f;

        /// <summary>Vertical spacing used between rows in the deleted files footer.</summary>
        public const float ProjectDeletedFooterRowSpacing = 2f;

        /// <summary>Maximum number of deleted entries shown at once inside the Project footer.</summary>
        public const int ProjectDeletedFooterMaxVisibleItems = 8;

        #endregion
    }
}
