namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Identifies the semantic role of a unified diff line.
    /// </summary>
    public enum GitDiffLineKind
    {
        /// <summary>An unchanged context line.</summary>
        Context,

        /// <summary>A line added in the target state.</summary>
        Added,

        /// <summary>A line removed from the source state.</summary>
        Removed,

        /// <summary>A marker such as the missing-newline annotation.</summary>
        Metadata
    }
}
