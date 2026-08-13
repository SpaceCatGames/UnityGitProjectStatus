using UnityEditor;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Declares menu commands that trigger refreshes and switch refresh modes.
    /// The menu forwards user intent to the shared settings and cache layers.
    /// Validation handlers keep refresh-mode checkmarks synchronized with current state.
    /// </summary>
    internal static class UnityGitStatusRefreshMenus
    {
        [MenuItem(Constants.RefreshMenuPath, false, 2000)]
        private static void RefreshNow() => GitStatusCache.RefreshNow();

        [MenuItem(Constants.RefreshModeManualMenuPath, false, 2001)]
        private static void SetManualRefreshMode() => UnityGitStatusSettings.ApplyRefreshMode(GitRefreshMode.ManualOnly);

        [MenuItem(Constants.RefreshModeTimedMenuPath, false, 2002)]
        private static void SetTimedRefreshMode() => UnityGitStatusSettings.ApplyRefreshMode(GitRefreshMode.Timed);

        [MenuItem(Constants.RefreshModeEventDrivenMenuPath, false, 2003)]
        private static void SetEventDrivenRefreshMode() => UnityGitStatusSettings.ApplyRefreshMode(GitRefreshMode.EventDriven);

        [MenuItem(Constants.RefreshModeManualMenuPath, true)]
        private static bool SetManualRefreshModeValidate() =>
            ValidateRefreshModeMenuItem(GitRefreshMode.ManualOnly, Constants.RefreshModeManualMenuPath);

        [MenuItem(Constants.RefreshModeTimedMenuPath, true)]
        private static bool SetTimedRefreshModeValidate() =>
            ValidateRefreshModeMenuItem(GitRefreshMode.Timed, Constants.RefreshModeTimedMenuPath);

        [MenuItem(Constants.RefreshModeEventDrivenMenuPath, true)]
        private static bool SetEventDrivenRefreshModeValidate() =>
            ValidateRefreshModeMenuItem(GitRefreshMode.EventDriven, Constants.RefreshModeEventDrivenMenuPath);

        private static bool ValidateRefreshModeMenuItem(GitRefreshMode refreshMode, string menuPath)
        {
            Menu.SetChecked(menuPath, UnityGitStatusSettings.RefreshMode == refreshMode);
            return true;
        }
    }
}
