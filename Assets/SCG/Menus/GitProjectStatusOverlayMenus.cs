using UnityEditor;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Declares menu commands that control badge visibility and appearance.
    /// These menu items are thin wrappers around the shared settings layer.
    /// Validation handlers keep Unity checkmarks in sync with the stored preferences.
    /// </summary>
    internal static class GitProjectStatusOverlayMenus
    {
        [MenuItem(Constants.ProjectOverlaysMenuPath, false, 2010)]
        private static void ToggleProjectOverlays() =>
            GitProjectStatusSettings.ProjectOverlaysEnabled = !GitProjectStatusSettings.ProjectOverlaysEnabled;

        [MenuItem(Constants.ProjectOverlaysMenuPath, true)]
        private static bool ToggleProjectOverlaysValidate()
        {
            Menu.SetChecked(Constants.ProjectOverlaysMenuPath, GitProjectStatusSettings.ProjectOverlaysEnabled);
            return true;
        }

        [MenuItem(Constants.RightAlignedBadgesMenuPath, false, 2011)]
        private static void ToggleRightAlignedBadges() =>
            GitProjectStatusSettings.RightAlignedBadges = !GitProjectStatusSettings.RightAlignedBadges;

        [MenuItem(Constants.RightAlignedBadgesMenuPath, true)]
        private static bool ToggleRightAlignedBadgesValidate()
        {
            Menu.SetChecked(Constants.RightAlignedBadgesMenuPath, GitProjectStatusSettings.RightAlignedBadges);
            return GitProjectStatusSettings.ProjectOverlaysEnabled;
        }

        [MenuItem(Constants.InspectorBadgeMenuPath, false, 2012)]
        private static void ToggleInspectorBadge() =>
            GitProjectStatusSettings.InspectorBadgeEnabled = !GitProjectStatusSettings.InspectorBadgeEnabled;

        [MenuItem(Constants.InspectorBadgeMenuPath, true)]
        private static bool ToggleInspectorBadgeValidate()
        {
            Menu.SetChecked(Constants.InspectorBadgeMenuPath, GitProjectStatusSettings.InspectorBadgeEnabled);
            return true;
        }

        [MenuItem(Constants.CalcModeMenuPath, false, 2013)]
        private static void ToggleCalcMode() =>
            GitProjectStatusSettings.CalcMode = !GitProjectStatusSettings.CalcMode;

        [MenuItem(Constants.CalcModeMenuPath, true)]
        private static bool ToggleCalcModeValidate()
        {
            Menu.SetChecked(Constants.CalcModeMenuPath, GitProjectStatusSettings.CalcMode);
            return GitProjectStatusSettings.ProjectOverlaysEnabled || GitProjectStatusSettings.InspectorBadgeEnabled;
        }

        [MenuItem(Constants.ShowDeletedFilesInProjectMenuPath, false, 2014)]
        private static void ToggleDeletedFilesInProject() =>
            GitProjectStatusSettings.ShowDeletedFilesInProject = !GitProjectStatusSettings.ShowDeletedFilesInProject;

        [MenuItem(Constants.ShowDeletedFilesInProjectMenuPath, true)]
        private static bool ToggleDeletedFilesInProjectValidate()
        {
            Menu.SetChecked(Constants.ShowDeletedFilesInProjectMenuPath, GitProjectStatusSettings.ShowDeletedFilesInProject);
            return GitProjectStatusSettings.ProjectOverlaysEnabled;
        }

        [MenuItem(Constants.ShowLeftPaneOverlaysInTwoColumnMenuPath, false, 2015)]
        private static void ToggleLeftPaneOverlaysInTwoColumn() =>
            GitProjectStatusSettings.ShowLeftPaneOverlaysInTwoColumn = !GitProjectStatusSettings.ShowLeftPaneOverlaysInTwoColumn;

        [MenuItem(Constants.ShowLeftPaneOverlaysInTwoColumnMenuPath, true)]
        private static bool ToggleLeftPaneOverlaysInTwoColumnValidate()
        {
            Menu.SetChecked(
                Constants.ShowLeftPaneOverlaysInTwoColumnMenuPath,
                GitProjectStatusSettings.ShowLeftPaneOverlaysInTwoColumn);
            return GitProjectStatusSettings.ProjectOverlaysEnabled;
        }
    }
}
