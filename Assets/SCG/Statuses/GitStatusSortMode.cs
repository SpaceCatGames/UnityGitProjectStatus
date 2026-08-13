namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Defines the available ordering modes for changed paths in the Git Status window.
    /// </summary>
    internal enum GitStatusSortMode
    {
        /// <summary>Sort paths alphabetically in ascending order.</summary>
        PathAscending,

        /// <summary>Sort paths alphabetically in descending order.</summary>
        PathDescending,

        /// <summary>Sort file names alphabetically in ascending order.</summary>
        FileNameAscending,

        /// <summary>Sort file names alphabetically in descending order.</summary>
        FileNameDescending,

        /// <summary>Group entries by Git status and then sort each group by path.</summary>
        FileStatus
    }
}
