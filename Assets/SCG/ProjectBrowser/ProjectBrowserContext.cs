using UnityEngine;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Stores the resolved Project Browser geometry and layout metadata for one GUI callback.
    /// The context is produced by reflection-based resolver logic and reused by overlay drawing code.
    /// It stays internal because it directly exposes Unity internal window objects and pane heuristics.
    /// </summary>
    internal readonly struct ProjectBrowserContext
    {
        #region Properties

        /// <summary>Resolved Unity internal ProjectBrowser instance associated with the current callback.</summary>
        internal object ProjectBrowser { get; }

        /// <summary>Main list-area rect resolved from the current Project Browser.</summary>
        internal Rect ListAreaRect { get; }

        /// <summary>Left tree-pane rect resolved from the current Project Browser.</summary>
        internal Rect TreeViewRect { get; }

        /// <summary>Current GUI visible rect used to classify the active pane.</summary>
        internal Rect VisibleRect { get; }

        /// <summary>Gets whether the Project Browser is currently using the two-column layout.</summary>
        internal bool IsTwoColumns { get; }

        /// <summary>Resolved pane associated with the current callback row.</summary>
        internal ProjectBrowserPane Pane { get; }

        /// <summary>Hash-based identifier used only for transient matching between callbacks.</summary>
        internal int InstanceId => ProjectBrowser?.GetHashCode() ?? 0;

        /// <summary>Gets whether the current callback belongs to the left tree pane in two-column mode.</summary>
        internal bool IsTreePane => Pane == ProjectBrowserPane.TwoColumnLeftTree;

        /// <summary>Gets whether the current callback belongs to the primary item list pane.</summary>
        internal bool IsRightPane => Pane is ProjectBrowserPane.OneColumn or ProjectBrowserPane.TwoColumnRightList;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new Project Browser callback context.
        /// All geometry and pane information must already be resolved before construction.
        /// The struct only stores the resolved values and does not perform further reflection.
        /// </summary>
        /// <param name="projectBrowser">Resolved Unity internal ProjectBrowser instance.</param>
        /// <param name="listAreaRect">Main list-area rect resolved from the Project Browser.</param>
        /// <param name="treeViewRect">Left tree-pane rect resolved from the Project Browser.</param>
        /// <param name="visibleRect">Current GUI visible rect used for pane matching.</param>
        /// <param name="isTwoColumns">Whether the Project Browser is using two-column layout.</param>
        /// <param name="pane">Resolved pane classification for the current callback.</param>
        internal ProjectBrowserContext(
            object projectBrowser,
            Rect listAreaRect,
            Rect treeViewRect,
            Rect visibleRect,
            bool isTwoColumns,
            ProjectBrowserPane pane)
        {
            ProjectBrowser = projectBrowser;
            ListAreaRect = listAreaRect;
            TreeViewRect = treeViewRect;
            VisibleRect = visibleRect;
            IsTwoColumns = isTwoColumns;
            Pane = pane;
        }

        #endregion
    }
}
