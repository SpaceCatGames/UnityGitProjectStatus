namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Represents one structurally immutable line inside a unified diff hunk with mutable UI selection state.
    /// Line numbers use zero when the line does not exist on that side.
    /// </summary>
    public sealed class GitDiffLine
    {
        #region Properties

        /// <summary>The line classification.</summary>
        public GitDiffLineKind Kind { get; }

        /// <summary>The content without the unified diff prefix, except metadata lines which retain their leading backslash.</summary>
        public string Content { get; }

        /// <summary>The one-based source line number, or zero.</summary>
        public int OldLineNumber { get; }

        /// <summary>The one-based target line number, or zero.</summary>
        public int NewLineNumber { get; }

        /// <summary>Whether this changed line is selected by the editor UI.</summary>
        public bool IsSelected { get; set; }

        /// <summary>Whether this line can participate in a partial operation.</summary>
        public bool IsSelectable => Kind is GitDiffLineKind.Added or GitDiffLineKind.Removed;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a structurally immutable diff line with mutable UI selection state.
        /// </summary>
        /// <param name="kind">The semantic line kind.</param>
        /// <param name="content">The content without its diff prefix, or the complete metadata line including its leading backslash.</param>
        /// <param name="oldLineNumber">The one-based source line number, or zero.</param>
        /// <param name="newLineNumber">The one-based target line number, or zero.</param>
        public GitDiffLine(GitDiffLineKind kind, string content, int oldLineNumber, int newLineNumber)
        {
            Kind = kind;
            Content = content ?? string.Empty;
            OldLineNumber = oldLineNumber;
            NewLineNumber = newLineNumber;
        }

        #endregion
    }
}
