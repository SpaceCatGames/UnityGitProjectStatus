using System.Collections.Generic;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Represents one parsed unified diff hunk.
    /// </summary>
    public sealed class GitDiffHunk
    {
        #region Properties

        /// <summary>The original hunk header.</summary>
        public string Header { get; }

        /// <summary>The one-based source start line.</summary>
        public int OldStart { get; }

        /// <summary>The one-based target start line.</summary>
        public int NewStart { get; }

        /// <summary>The lines contained by the hunk.</summary>
        public IReadOnlyList<GitDiffLine> Lines { get; }

        #endregion

        #region Construction

        /// <summary>
        /// Creates a parsed hunk.
        /// </summary>
        /// <param name="header">The original hunk header.</param>
        /// <param name="oldStart">The source start line.</param>
        /// <param name="newStart">The target start line.</param>
        /// <param name="lines">The parsed hunk lines.</param>
        public GitDiffHunk(string header, int oldStart, int newStart, IReadOnlyList<GitDiffLine> lines)
        {
            Header = header ?? string.Empty;
            OldStart = oldStart;
            NewStart = newStart;
            Lines = lines ?? new List<GitDiffLine>();
        }

        #endregion
    }
}
