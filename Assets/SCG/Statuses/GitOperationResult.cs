namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Describes the outcome of a Git diff or mutation operation.
    /// </summary>
    internal sealed class GitOperationResult
    {
        #region Properties

        /// <summary>Whether the operation completed successfully.</summary>
        internal bool Success { get; }

        /// <summary>Captured standard output.</summary>
        internal string Output { get; }

        /// <summary>A user-facing error message.</summary>
        internal string Error { get; }

        #endregion

        #region Construction

        /// <summary>
        /// Creates an operation result.
        /// </summary>
        /// <param name="success">Whether the operation succeeded.</param>
        /// <param name="output">Captured output.</param>
        /// <param name="error">A user-facing error.</param>
        internal GitOperationResult(bool success, string output, string error)
        {
            Success = success;
            Output = output ?? string.Empty;
            Error = error ?? string.Empty;
        }

        #endregion
    }
}
