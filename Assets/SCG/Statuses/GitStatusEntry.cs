namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Represents a single normalized Git status entry tracked by the package.
    /// The entry stores the original and display paths together with derived metadata used by overlays.
    /// Instances are immutable so cache snapshots can be shared safely between editor callbacks.
    /// </summary>
    public sealed class GitStatusEntry
    {
        #region Properties

        /// <summary>Normalized path reported by Git for the entry.</summary>
        public string Path { get; }

        /// <summary>Normalized source path used by rename and copy records.</summary>
        public string OriginalPath { get; }

        /// <summary>Repository-relative path reported by Git before Unity-project path remapping.</summary>
        internal string RepositoryPath { get; }

        /// <summary>Repository-relative source path reported by Git for rename and copy records.</summary>
        internal string OriginalRepositoryPath { get; }

        /// <summary>Simplified status classification used by the package UI.</summary>
        public GitStatusKind Kind { get; }

        /// <summary>Whether the entry references a Unity meta file.</summary>
        public bool IsMeta { get; }

        /// <summary>Whether the entry was created as a folder aggregate instead of a direct file status.</summary>
        public bool IsFolderAggregate { get; }

        /// <summary>Visible Unity path used by the window and Project overlay.</summary>
        public string DisplayPath { get; }

        /// <summary>Whether the entry represents a deleted path.</summary>
        public bool IsDeleted => Kind == GitStatusKind.Deleted;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new immutable Git status entry.
        /// The constructor normalizes all path-like values to use Unity-style forward slashes.
        /// DisplayPath falls back to Path when an explicit visible path is not provided.
        /// </summary>
        /// <param name="path">Repository-relative or project-relative path associated with the entry.</param>
        /// <param name="originalPath">Original path used for rename and copy records.</param>
        /// <param name="kind">Simplified Git status classification.</param>
        /// <param name="isMeta">Whether the record points to a Unity meta file.</param>
        /// <param name="isFolderAggregate">Whether the record is an aggregated folder status rather than a direct asset status.</param>
        /// <param name="displayPath">Visible Unity path used by overlays and the status window.</param>
        public GitStatusEntry(
            string path,
            string originalPath,
            GitStatusKind kind,
            bool isMeta,
            bool isFolderAggregate,
            string displayPath)
            : this(path, originalPath, kind, isMeta, isFolderAggregate, displayPath, path, originalPath)
        {
        }

        /// <summary>
        /// Creates an immutable Git status entry while preserving both Unity-project and repository-relative paths.
        /// </summary>
        /// <param name="path">Project-relative path used by Unity-facing UI.</param>
        /// <param name="originalPath">Project-relative source path used by rename and copy records.</param>
        /// <param name="kind">Simplified Git status classification.</param>
        /// <param name="isMeta">Whether the record points to a Unity meta file.</param>
        /// <param name="isFolderAggregate">Whether the record is an aggregated folder status.</param>
        /// <param name="displayPath">Visible Unity path used by overlays and the status window.</param>
        /// <param name="repositoryPath">Path relative to the repository root.</param>
        /// <param name="originalRepositoryPath">Source path relative to the repository root.</param>
        internal GitStatusEntry(
            string path,
            string originalPath,
            GitStatusKind kind,
            bool isMeta,
            bool isFolderAggregate,
            string displayPath,
            string repositoryPath,
            string originalRepositoryPath)
        {
            Path = NormalizePath(path);
            OriginalPath = NormalizePath(originalPath);
            RepositoryPath = NormalizePath(repositoryPath);
            OriginalRepositoryPath = NormalizePath(originalRepositoryPath);
            Kind = kind;
            IsMeta = isMeta;
            IsFolderAggregate = isFolderAggregate;
            DisplayPath = string.IsNullOrEmpty(displayPath) ? Path : NormalizePath(displayPath);
        }

        #endregion

        #region Factory Helpers

        /// <summary>
        /// Creates a copy of the entry with a different visible path and aggregate flag.
        /// This is used when remapping Git results to Unity-visible asset or folder paths.
        /// The source entry remains unchanged.
        /// </summary>
        /// <param name="displayPath">Visible Unity path that should be shown for the entry.</param>
        /// <param name="isFolderAggregate">Whether the resulting entry should be treated as a folder aggregate.</param>
        /// <returns>A new immutable entry carrying the updated display metadata.</returns>
        public GitStatusEntry WithDisplayPath(string displayPath, bool isFolderAggregate) =>
            new(
                Path,
                OriginalPath,
                Kind,
                IsMeta,
                isFolderAggregate,
                displayPath,
                RepositoryPath,
                OriginalRepositoryPath);

        #endregion

        #region Path Helpers

        /// <summary>
        /// Normalizes a path to use forward slashes and an empty string fallback.
        /// This keeps path comparisons stable across Windows and Unix-like environments.
        /// Null input is converted to an empty string.
        /// </summary>
        /// <param name="path">Path value to normalize.</param>
        /// <returns>The normalized path using forward slashes, or an empty string.</returns>
        public static string NormalizePath(string path) =>
            string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');

        /// <summary>
        /// Determines whether a path points to a Unity meta file.
        /// Meta files are remapped to their visible asset or folder whenever possible.
        /// The suffix comparison follows the current platform path semantics.
        /// </summary>
        /// <param name="path">Path value to inspect.</param>
        /// <returns>True when the path ends with the Unity meta file suffix; otherwise false.</returns>
        public static bool IsMetaPath(string path) =>
            !string.IsNullOrEmpty(path) &&
            path.EndsWith(".meta", GitPathComparer.Comparison);

        #endregion
    }
}
