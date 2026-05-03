using System;
using System.Collections.Generic;
using System.Linq;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Stores the immutable Git status state shared by overlays and the status window.
    /// A new snapshot is created after each completed refresh and then reused by editor callbacks.
    /// It also carries repository availability and the last refresh error.
    /// </summary>
    public sealed class GitStatusSnapshot
    {
        #region Static Members

        /// <summary>Empty snapshot used before the first refresh completes.</summary>
        public static readonly GitStatusSnapshot Empty = new(
            false,
            string.Empty,
            default,
            string.Empty,
            0,
            0,
            new Dictionary<string, GitStatusEntry>(GitPathComparer.Comparer),
            new List<GitStatusEntry>(),
            new List<GitStatusEntry>(),
            new Dictionary<string, IReadOnlyList<GitStatusEntry>>(GitPathComparer.Comparer));

        #endregion

        #region Properties

        /// <summary>Whether a Git repository was successfully resolved for the current Unity project.</summary>
        public bool RepositoryDetected { get; }

        /// <summary>Current Git branch name, or an empty string when unavailable.</summary>
        public string Branch { get; }

        /// <summary>Timestamp of the last refresh attempt that produced this snapshot.</summary>
        public DateTime LastRefreshTime { get; }

        /// <summary>User-facing error associated with the current snapshot.</summary>
        public string LastError { get; }

        /// <summary>Number of changed files tracked outside the Unity Assets root.</summary>
        public int OutsideAssetsChangedCount { get; }

        /// <summary>Number of deleted files tracked outside the Unity Assets root.</summary>
        public int OutsideAssetsDeletedCount { get; }

        /// <summary>Visible status entries indexed by Unity asset path.</summary>
        public IReadOnlyDictionary<string, GitStatusEntry> AssetStatuses { get; }

        /// <summary>Direct status entries parsed from Git output after Unity path normalization.</summary>
        public IReadOnlyList<GitStatusEntry> Entries { get; }

        /// <summary>Deleted entries tracked separately from visible Project asset rows.</summary>
        public IReadOnlyList<GitStatusEntry> DeletedEntries { get; }

        /// <summary>Deleted entries grouped by their direct Unity folder path.</summary>
        public IReadOnlyDictionary<string, IReadOnlyList<GitStatusEntry>> DeletedEntriesByFolder { get; }

        /// <summary>Number of deleted entries tracked by the snapshot.</summary>
        public int DeletedCount => DeletedEntries.Count;

        /// <summary>Number of direct changed entries excluding deleted entries.</summary>
        public int ChangedEntriesCount => Entries.Count(t => !t.IsDeleted);

        /// <summary>Number of direct changed meta entries excluding deleted entries.</summary>
        public int ChangedMetaEntriesCount => Entries.Count(entry => !entry.IsDeleted && entry.IsMeta);

        /// <summary>Number of deleted direct entries that point to Unity meta files.</summary>
        public int DeletedMetaEntriesCount => DeletedEntries.Count(t => t.IsMeta);

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new immutable snapshot of the current Git status state.
        /// Collections are expected to be fully materialized before the snapshot is constructed.
        /// The snapshot itself does not mutate them after assignment.
        /// </summary>
        /// <param name="repositoryDetected">Whether a valid Git repository was detected for the Unity project.</param>
        /// <param name="branch">Current branch name resolved from Git.</param>
        /// <param name="lastRefreshTime">Timestamp of the refresh that produced the snapshot.</param>
        /// <param name="lastError">User-facing error associated with the refresh result.</param>
        /// <param name="outsideAssetsChangedCount">Number of changed files outside the Unity Assets root.</param>
        /// <param name="outsideAssetsDeletedCount">Number of deleted files outside the Unity Assets root.</param>
        /// <param name="assetStatuses">Visible status entries indexed by Unity asset path.</param>
        /// <param name="entries">Direct non-aggregated entries parsed from Git output.</param>
        /// <param name="deletedEntries">Deleted entries tracked separately from visible Project asset rows.</param>
        /// <param name="deletedEntriesByFolder">Deleted entries grouped by their direct Unity folder path.</param>
        public GitStatusSnapshot(
            bool repositoryDetected,
            string branch,
            DateTime lastRefreshTime,
            string lastError,
            int outsideAssetsChangedCount,
            int outsideAssetsDeletedCount,
            Dictionary<string, GitStatusEntry> assetStatuses,
            List<GitStatusEntry> entries,
            List<GitStatusEntry> deletedEntries,
            Dictionary<string, IReadOnlyList<GitStatusEntry>> deletedEntriesByFolder)
        {
            RepositoryDetected = repositoryDetected;
            Branch = branch ?? string.Empty;
            LastRefreshTime = lastRefreshTime;
            LastError = lastError ?? string.Empty;
            OutsideAssetsChangedCount = Math.Max(0, outsideAssetsChangedCount);
            OutsideAssetsDeletedCount = Math.Max(0, outsideAssetsDeletedCount);
            AssetStatuses = assetStatuses ?? new Dictionary<string, GitStatusEntry>(GitPathComparer.Comparer);
            Entries = entries ?? new List<GitStatusEntry>();
            DeletedEntries = deletedEntries ?? new List<GitStatusEntry>();
            DeletedEntriesByFolder =
                deletedEntriesByFolder ?? new Dictionary<string, IReadOnlyList<GitStatusEntry>>(GitPathComparer.Comparer);
        }

        #endregion

        #region Factory Helpers

        /// <summary>
        /// Resolves deleted entries associated with a specific Unity folder path.
        /// Only direct deleted children of that folder are returned.
        /// Unknown folders return an empty list.
        /// </summary>
        /// <param name="folderPath">Unity folder path that should be queried.</param>
        /// <returns>Deleted entries directly associated with the requested folder.</returns>
        public IReadOnlyList<GitStatusEntry> GetDeletedEntriesForFolder(string folderPath)
        {
            var normalizedFolderPath = GitStatusEntry.NormalizePath(folderPath);
            return string.IsNullOrEmpty(normalizedFolderPath) ||
                !DeletedEntriesByFolder.TryGetValue(normalizedFolderPath, out var entries)
                    ? Array.Empty<GitStatusEntry>()
                    : entries;
        }

        /// <summary>
        /// Creates a failed snapshot stamped with the current local time.
        /// This is used when a refresh cannot resolve Git state successfully.
        /// The returned snapshot carries no visible entries.
        /// </summary>
        /// <param name="error">User-facing error message describing the failure.</param>
        /// <returns>An empty failed snapshot with the provided error message.</returns>
        public static GitStatusSnapshot CreateFailed(string error) => CreateFailed(error, DateTime.Now);

        /// <summary>
        /// Creates a failed snapshot stamped with a specific refresh time.
        /// This allows the cache to preserve the completion time of the failed refresh operation.
        /// The returned snapshot carries no visible entries.
        /// </summary>
        /// <param name="error">User-facing error message describing the failure.</param>
        /// <param name="lastRefreshTime">Timestamp that should be associated with the failed snapshot.</param>
        /// <returns>An empty failed snapshot with the provided error and timestamp.</returns>
        public static GitStatusSnapshot CreateFailed(string error, DateTime lastRefreshTime) =>
            new(
                false,
                string.Empty,
                lastRefreshTime,
                error,
                0,
                0,
                new Dictionary<string, GitStatusEntry>(GitPathComparer.Comparer),
                new List<GitStatusEntry>(),
                new List<GitStatusEntry>(),
                new Dictionary<string, IReadOnlyList<GitStatusEntry>>(GitPathComparer.Comparer));

        #endregion
    }
}
