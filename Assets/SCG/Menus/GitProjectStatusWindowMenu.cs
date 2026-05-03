using UnityEditor;
using UnityEngine;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Declares the menu command used to open the Git status window.
    /// The class exists only to keep menu wiring separate from the window implementation.
    /// Unity discovers the command through the MenuItem attribute.
    /// </summary>
    internal static class GitProjectStatusWindowMenu
    {
        [MenuItem(Constants.WindowMenuPath, false, 3000)]
        private static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<GitProjectStatusWindow>();
            window.minSize = new Vector2(Constants.WindowMinWidth, Constants.WindowMinHeight);
            window.Show();
        }
    }
}
