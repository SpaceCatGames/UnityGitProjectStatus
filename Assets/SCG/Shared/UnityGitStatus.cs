using UnityEditor;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Bootstraps the package when Unity loads the editor domain.
    /// The static constructor wires Project-window and Inspector callbacks once per domain reload.
    /// Import worker processes are excluded because they should not draw editor UI.
    /// </summary>
    [InitializeOnLoad]
    internal static class UnityGitStatus
    {
        static UnityGitStatus()
        {
            if (AssetDatabase.IsAssetImportWorkerProcess())
                return;

            EditorApplication.projectWindowItemOnGUI -= GitProjectOverlayCallbacks.OnProjectWindowItemGui;
            EditorApplication.projectWindowItemOnGUI += GitProjectOverlayCallbacks.OnProjectWindowItemGui;

            Editor.finishedDefaultHeaderGUI -= GitInspectorBadgeCallbacks.OnFinishedDefaultHeaderGui;
            Editor.finishedDefaultHeaderGUI += GitInspectorBadgeCallbacks.OnFinishedDefaultHeaderGui;
        }
    }
}
