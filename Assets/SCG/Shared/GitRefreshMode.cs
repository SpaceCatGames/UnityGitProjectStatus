namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Defines how the package refreshes Git status.
    /// Manual-only mode disables automatic refresh.
    /// Timed mode refreshes on a configurable interval.
    /// Event-driven mode uses selected editor lifecycle events.
    /// </summary>
    public enum GitRefreshMode
    {
        /// <summary>Refresh only when the user requests it explicitly.</summary>
        ManualOnly,

        /// <summary>Refresh automatically on a configurable timer.</summary>
        Timed,

        /// <summary>Refresh automatically from editor lifecycle events.</summary>
        EventDriven
    }
}
