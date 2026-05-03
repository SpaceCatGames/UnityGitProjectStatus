namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Identifies which Project Browser pane is currently being processed by the overlay callback.
    /// The value is used to keep badge visibility and deleted footer behavior consistent between layouts.
    /// It also separates the two-column tree pane from the right list pane.
    /// </summary>
    internal enum ProjectBrowserPane
    {
        /// <summary>
        /// The current pane could not be resolved reliably.
        /// This value is used only as a defensive fallback.
        /// </summary>
        Unknown,

        /// <summary>
        /// The Project window is currently using the single-column layout.
        /// Badges should behave like the standard one-column Project view in this mode.
        /// </summary>
        OneColumn,

        /// <summary>
        /// The callback is currently drawing inside the left tree pane of the two-column Project layout.
        /// This pane is affected by the left-pane overlay toggle.
        /// </summary>
        TwoColumnLeftTree,

        /// <summary>
        /// The callback is currently drawing inside the right list pane of the two-column Project layout.
        /// This pane remains active even when left-pane overlays are disabled.
        /// </summary>
        TwoColumnRightList
    }
}
