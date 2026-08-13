namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Represents the raw result of a Git status refresh run.
    /// The result includes repository metadata, porcelain output, process failures, and timeout information.
    /// The cache layer uses this data to build immutable editor snapshots.
    /// </summary>
    internal sealed class GitStatusRunResult
    {
        /// <summary>Resolved repository root returned by Git.</summary>
        public string RepositoryRoot { get; set; }

        /// <summary>Relative Unity project path inside the repository.</summary>
        public string ProjectPathInRepository { get; set; }

        /// <summary>Raw porcelain status output returned by Git.</summary>
        public string StatusOutput { get; set; }

        /// <summary>Standard error output produced by the Git process.</summary>
        public string StatusError { get; set; }

        /// <summary>Exit code returned by the Git status command.</summary>
        public int StatusExitCode { get; set; }

        /// <summary>Whether the Git command timed out before completion.</summary>
        public bool TimedOut { get; set; }

        /// <summary>Current branch name returned by Git when available.</summary>
        public string Branch { get; set; }

        /// <summary>Process-level failure message produced before Git could return a result.</summary>
        public string Error { get; set; }

        /// <summary>Whether the Git run completed successfully and produced a usable result.</summary>
        public bool Succeeded => !TimedOut && StatusExitCode == 0 && string.IsNullOrEmpty(Error);
    }
}
