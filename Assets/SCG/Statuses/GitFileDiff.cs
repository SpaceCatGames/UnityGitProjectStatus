using System.Collections.Generic;
using System.Linq;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Represents a parsed file diff for either the index or working tree.
    /// It exposes read-only structural data while individual line selection remains UI state.
    /// </summary>
    public sealed class GitFileDiff
    {
        #region Properties

        /// <summary>The repository-relative file path.</summary>
        public string Path { get; }

        /// <summary>The repository state represented by the diff.</summary>
        public GitDiffSide Side { get; }

        /// <summary>The complete file-level patch header.</summary>
        public IReadOnlyList<string> HeaderLines { get; }

        /// <summary>The parsed hunks.</summary>
        public IReadOnlyList<GitDiffHunk> Hunks { get; }

        /// <summary>Whether Git reports a binary patch.</summary>
        public bool IsBinary { get; }

        /// <summary>Whether the change creates a new file.</summary>
        public bool IsNewFile { get; }

        /// <summary>Whether the change deletes a file.</summary>
        public bool IsDeletedFile { get; }

        /// <summary>Whether the change renames or copies a file.</summary>
        public bool IsRenameOrCopy { get; }

        /// <summary>Whether the path is in a conflicted Git state that cannot use a two-way line patch.</summary>
        public bool IsConflicted { get; }

        /// <summary>Whether the diff requires whole-file actions.</summary>
        public bool RequiresWholeFileAction =>
            IsBinary || IsNewFile || IsDeletedFile || IsRenameOrCopy || IsConflicted || Hunks.Count == 0;

        /// <summary>Whether at least one changed line is selected.</summary>
        public bool HasSelection => Hunks.SelectMany(hunk => hunk.Lines).Any(line => line.IsSelectable && line.IsSelected);

        /// <summary>The unmodified patch returned by Git for this file and side.</summary>
        internal string OriginalPatch { get; set; } = string.Empty;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a parsed file diff.
        /// </summary>
        /// <param name="path">The repository-relative path.</param>
        /// <param name="side">The represented repository side.</param>
        /// <param name="headerLines">The file patch header.</param>
        /// <param name="hunks">The parsed hunks.</param>
        /// <param name="isBinary">Whether the diff is binary.</param>
        /// <param name="isNewFile">Whether the file is new.</param>
        /// <param name="isDeletedFile">Whether the file is deleted.</param>
        /// <param name="isRenameOrCopy">Whether the change is a rename or copy.</param>
        /// <param name="isConflicted">Whether the path is in a conflicted Git state.</param>
        public GitFileDiff(
            string path,
            GitDiffSide side,
            IReadOnlyList<string> headerLines,
            IReadOnlyList<GitDiffHunk> hunks,
            bool isBinary,
            bool isNewFile,
            bool isDeletedFile,
            bool isRenameOrCopy,
            bool isConflicted = false)
        {
            Path = path ?? string.Empty;
            Side = side;
            HeaderLines = headerLines ?? new List<string>();
            Hunks = hunks ?? new List<GitDiffHunk>();
            IsBinary = isBinary;
            IsNewFile = isNewFile;
            IsDeletedFile = isDeletedFile;
            IsRenameOrCopy = isRenameOrCopy;
            IsConflicted = isConflicted;
        }

        #endregion
    }
}
