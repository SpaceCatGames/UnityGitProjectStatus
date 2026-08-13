namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Identifies the repository state represented by a diff.
    /// </summary>
    public enum GitDiffSide
    {
        /// <summary>The diff compares HEAD with the index.</summary>
        Staged,

        /// <summary>The diff compares the index with the working tree.</summary>
        Unstaged
    }
}
