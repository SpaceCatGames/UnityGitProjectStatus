using UnityEditor;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Declares menu commands that control badge visibility and appearance.
    /// These menu items are thin wrappers around the shared settings layer.
    /// Validation handlers keep Unity checkmarks in sync with the stored preferences.
    /// </summary>
    internal static class UnityGitStatusOverlayMenus
    {
        [MenuItem(Constants.ProjectOverlaysMenuPath, false, 2010)]
        private static void ToggleProjectOverlays() =>
            UnityGitStatusSettings.ProjectOverlaysEnabled = !UnityGitStatusSettings.ProjectOverlaysEnabled;

        [MenuItem(Constants.ProjectOverlaysMenuPath, true)]
        private static bool ToggleProjectOverlaysValidate()
        {
            Menu.SetChecked(Constants.ProjectOverlaysMenuPath, UnityGitStatusSettings.ProjectOverlaysEnabled);
            return true;
        }

        [MenuItem(Constants.RightAlignedBadgesMenuPath, false, 2011)]
        private static void ToggleRightAlignedBadges() =>
            UnityGitStatusSettings.RightAlignedBadges = !UnityGitStatusSettings.RightAlignedBadges;

        [MenuItem(Constants.RightAlignedBadgesMenuPath, true)]
        private static bool ToggleRightAlignedBadgesValidate()
        {
            Menu.SetChecked(Constants.RightAlignedBadgesMenuPath, UnityGitStatusSettings.RightAlignedBadges);
            return UnityGitStatusSettings.ProjectOverlaysEnabled;
        }

        [MenuItem(Constants.InspectorBadgeMenuPath, false, 2012)]
        private static void ToggleInspectorBadge() =>
            UnityGitStatusSettings.InspectorBadgeEnabled = !UnityGitStatusSettings.InspectorBadgeEnabled;

        [MenuItem(Constants.InspectorBadgeMenuPath, true)]
        private static bool ToggleInspectorBadgeValidate()
        {
            Menu.SetChecked(Constants.InspectorBadgeMenuPath, UnityGitStatusSettings.InspectorBadgeEnabled);
            return true;
        }

        [MenuItem(Constants.CalcModeMenuPath, false, 2013)]
        private static void ToggleCalcMode() =>
            UnityGitStatusSettings.CalcMode = !UnityGitStatusSettings.CalcMode;

        [MenuItem(Constants.CalcModeMenuPath, true)]
        private static bool ToggleCalcModeValidate()
        {
            Menu.SetChecked(Constants.CalcModeMenuPath, UnityGitStatusSettings.CalcMode);
            return UnityGitStatusSettings.ProjectOverlaysEnabled || UnityGitStatusSettings.InspectorBadgeEnabled;
        }

        [MenuItem(Constants.ShowDeletedFilesInProjectMenuPath, false, 2014)]
        private static void ToggleDeletedFilesInProject() =>
            UnityGitStatusSettings.ShowDeletedFilesInProject = !UnityGitStatusSettings.ShowDeletedFilesInProject;

        [MenuItem(Constants.ShowDeletedFilesInProjectMenuPath, true)]
        private static bool ToggleDeletedFilesInProjectValidate()
        {
            Menu.SetChecked(Constants.ShowDeletedFilesInProjectMenuPath, UnityGitStatusSettings.ShowDeletedFilesInProject);
            return UnityGitStatusSettings.ProjectOverlaysEnabled;
        }

        [MenuItem(Constants.ShowLeftPaneOverlaysInTwoColumnMenuPath, false, 2015)]
        private static void ToggleLeftPaneOverlaysInTwoColumn() =>
            UnityGitStatusSettings.ShowLeftPaneOverlaysInTwoColumn = !UnityGitStatusSettings.ShowLeftPaneOverlaysInTwoColumn;

        [MenuItem(Constants.ShowLeftPaneOverlaysInTwoColumnMenuPath, true)]
        private static bool ToggleLeftPaneOverlaysInTwoColumnValidate()
        {
            Menu.SetChecked(
                Constants.ShowLeftPaneOverlaysInTwoColumnMenuPath,
                UnityGitStatusSettings.ShowLeftPaneOverlaysInTwoColumn);
            return UnityGitStatusSettings.ProjectOverlaysEnabled;
        }
    }
}
