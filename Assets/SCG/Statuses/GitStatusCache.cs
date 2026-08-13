using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Caches the current Git status snapshot used by overlays and the status window.
    /// The cache executes Git off the main thread, reacts to selected editor lifecycle events, and remaps raw results to Unity paths.
    /// It also raises repaint notifications when the snapshot changes.
    /// </summary>
    [InitializeOnLoad]
    public static class GitStatusCache
    {
        #region Fields

        private static readonly object s_pendingLock = new();

        private static PendingRefreshResult pendingResult;
        private static bool refreshResultReady;
        private static bool refreshPending;
        private static bool wasEditorApplicationActive;
        private static GitRefreshMode lastObservedRefreshMode;
        private static int lastTimedRefreshIntervalSeconds;
        private static double nextTimedRefreshAt;
        private static string pendingRefreshReason = string.Empty;

        #endregion

        #region Initialization

        static GitStatusCache()
        {
            if (IsAssetImportWorkerProcess())
                return;

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;

            InitializeRefreshSettingsState();
            EditorApplication.delayCall += ScheduleStartupRefresh;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Raised after the current snapshot changes.
        /// The event is used by the status window to repaint itself when new data arrives.
        /// Project window repainting is triggered internally by the cache.
        /// </summary>
        public static event Action StatusChanged;

        /// <summary>Gets the latest immutable snapshot exposed to the editor UI.</summary>
        public static GitStatusSnapshot Snapshot { get; private set; } = GitStatusSnapshot.Empty;

        /// <summary>Gets whether a refresh is currently running in the background.</summary>
        public static bool IsRefreshInProgress { get; private set; }

        /// <summary>Gets whether the latest snapshot corresponds to a detected Git repository.</summary>
        public static bool RepositoryDetected => Snapshot.RepositoryDetected;

        /// <summary>Gets the repository root associated with the latest successful snapshot.</summary>
        internal static string RepositoryRoot { get; private set; } = string.Empty;

        /// <summary>
        /// Resolves the visible status entry associated with a Unity asset path.
        /// The lookup uses the currently published immutable snapshot.
        /// Null or empty paths return no entry.
        /// </summary>
        /// <param name="assetPath">Unity asset path that should be queried.</param>
        /// <returns>The visible entry associated with the asset path, or null when none exists.</returns>
        public static GitStatusEntry GetStatusForAssetPath(string assetPath) =>
            string.IsNullOrEmpty(assetPath)
                ? null
                : Snapshot.AssetStatuses.GetValueOrDefault(GitStatusEntry.NormalizePath(assetPath));

        /// <summary>
        /// Resolves deleted entries that belong directly to a Unity folder path.
        /// The data comes from the latest immutable snapshot built by the cache.
        /// Unknown folders return an empty list.
        /// </summary>
        /// <param name="folderAssetPath">Unity folder path that should be queried.</param>
        /// <returns>Deleted entries directly associated with the requested folder.</returns>
        public static IReadOnlyList<GitStatusEntry> GetDeletedEntriesForFolder(string folderAssetPath) =>
            Snapshot.GetDeletedEntriesForFolder(folderAssetPath);

        /// <summary>
        /// Queues an immediate refresh from the editor menu.
        /// This bypasses refresh mode restrictions but still respects the single-runner cache workflow.
        /// If another refresh is already running, one follow-up refresh remains pending.
        /// </summary>
        public static void RefreshNow()
        {
            if (!IsAssetImportWorkerProcess()) ScheduleRefresh("manual refresh");
        }

        /// <summary>
        /// Applies the current refresh settings to the in-memory scheduler state.
        /// Manual-only mode clears pending automatic refresh requests.
        /// Auto modes can optionally trigger an immediate refresh after the new settings are applied.
        /// </summary>
        /// <param name="refreshImmediately">True to enqueue an immediate refresh for the new automatic mode.</param>
        public static void ApplyRefreshSettingsChange(bool refreshImmediately)
        {
            if (IsAssetImportWorkerProcess())
                return;

            InitializeRefreshSettingsState();

            if (lastObservedRefreshMode == GitRefreshMode.ManualOnly)
            {
                refreshPending = false;
                pendingRefreshReason = string.Empty;
            }
            else if (refreshImmediately)
            {
                ScheduleRefresh("refresh settings changed");
            }

            RaiseStatusChanged();
        }

        /// <summary>
        /// Schedules a Git refresh.
        /// Only one refresh remains pending while another run is already in progress.
        /// This keeps the editor responsive without introducing polling behavior.
        /// </summary>
        /// <param name="reason">Human-readable reason associated with the refresh request.</param>
        public static void ScheduleRefresh(string reason)
        {
            if (IsAssetImportWorkerProcess())
                return;

            pendingRefreshReason = string.IsNullOrEmpty(reason) ? "refresh" : reason;
            refreshPending = true;
        }

        #endregion

        #region Editor Event Handlers

        private static void OnCompilationFinished(object _) => ScheduleEventDrivenRefresh("compilation finished");

        private static void OnEditorUpdate()
        {
            if (IsAssetImportWorkerProcess())
                return;

            if (EditorApplication.isCompiling)
                return;

            if (refreshResultReady) ApplyPendingResultIfAvailable();

            TrackRefreshSettings();
            TryScheduleEditorActivationRefresh();
            TryScheduleTimedRefresh();

            if (!refreshPending || IsRefreshInProgress)
                return;

            StartPendingRefresh();
        }

        #endregion

        #region Refresh Pipeline

        private static void InitializeRefreshSettingsState()
        {
            lastObservedRefreshMode = UnityGitStatusSettings.RefreshMode;
            lastTimedRefreshIntervalSeconds = UnityGitStatusSettings.TimedRefreshIntervalSeconds;
            wasEditorApplicationActive = InternalEditorUtility.isApplicationActive;
            nextTimedRefreshAt = lastObservedRefreshMode == GitRefreshMode.Timed
                ? EditorApplication.timeSinceStartup + lastTimedRefreshIntervalSeconds
                : 0d;
        }

        private static void TrackRefreshSettings()
        {
            var refreshMode = UnityGitStatusSettings.RefreshMode;
            var timedRefreshIntervalSeconds = UnityGitStatusSettings.TimedRefreshIntervalSeconds;

            if (refreshMode != lastObservedRefreshMode)
            {
                lastObservedRefreshMode = refreshMode;
                lastTimedRefreshIntervalSeconds = timedRefreshIntervalSeconds;
                wasEditorApplicationActive = InternalEditorUtility.isApplicationActive;
                nextTimedRefreshAt = refreshMode == GitRefreshMode.Timed
                    ? EditorApplication.timeSinceStartup + timedRefreshIntervalSeconds
                    : 0d;
            }

            if (timedRefreshIntervalSeconds == lastTimedRefreshIntervalSeconds) return;
            lastTimedRefreshIntervalSeconds = timedRefreshIntervalSeconds;

            if (refreshMode == GitRefreshMode.Timed)
            {
                nextTimedRefreshAt = EditorApplication.timeSinceStartup + timedRefreshIntervalSeconds;
            }
        }

        private static void TryScheduleEditorActivationRefresh()
        {
            var isEditorApplicationActive = InternalEditorUtility.isApplicationActive;

            if (lastObservedRefreshMode == GitRefreshMode.EventDriven &&
                isEditorApplicationActive &&
                !wasEditorApplicationActive)
            {
                ScheduleEventDrivenRefresh("editor activated");
            }

            wasEditorApplicationActive = isEditorApplicationActive;
        }

        private static void TryScheduleTimedRefresh()
        {
            if (lastObservedRefreshMode != GitRefreshMode.Timed)
                return;

            if (nextTimedRefreshAt <= 0d)
            {
                nextTimedRefreshAt = EditorApplication.timeSinceStartup + lastTimedRefreshIntervalSeconds;
            }

            if (EditorApplication.timeSinceStartup < nextTimedRefreshAt)
                return;

            nextTimedRefreshAt = EditorApplication.timeSinceStartup + lastTimedRefreshIntervalSeconds;
            ScheduleRefresh($"timed refresh ({lastTimedRefreshIntervalSeconds}s)");
        }

        private static void ScheduleStartupRefresh()
        {
            InitializeRefreshSettingsState();

            if (lastObservedRefreshMode == GitRefreshMode.ManualOnly)
            {
                return;
            }

            ScheduleRefresh("startup");
        }

        private static void StartPendingRefresh()
        {
            var reason = pendingRefreshReason;
            refreshPending = false;
            pendingRefreshReason = string.Empty;
            TryStartRefresh(reason);
        }

        private static void TryStartRefresh(string reason)
        {
            if (IsRefreshInProgress || EditorApplication.isCompiling || IsAssetImportWorkerProcess())
                return;

            var projectRoot = GetUnityProjectRoot();

            if (string.IsNullOrEmpty(projectRoot))
            {
                Snapshot = GitStatusSnapshot.CreateFailed("Unable to resolve Unity project root.");
                RaiseStatusChanged();
                return;
            }

            IsRefreshInProgress = true;

            GitStatusRunner.RunAsync(projectRoot, Constants.GitCommandTimeoutMilliseconds).ContinueWith(task =>
            {
                var result = new PendingRefreshResult
                {
                    ProjectRoot = projectRoot,
                    CompletedAt = DateTime.Now
                };

                try
                {
                    if (task.IsFaulted)
                    {
                        result.Error = task.Exception != null
                            ? task.Exception.GetBaseException().Message
                            : "Git refresh task failed.";
                    }
                    else if (task.IsCanceled)
                    {
                        result.Error = "Git refresh task was cancelled.";
                    }
                    else
                    {
                        result.RunResult = task.Result;

                        if (result.RunResult is { Succeeded: true })
                        {
                            result.Entries = GitStatusParser.Parse(result.RunResult.StatusOutput);
                        }
                    }
                }
                catch (Exception exception) when (
                    exception is ArgumentException or FormatException or InvalidOperationException)
                {
                    result.Error = $"{reason}: {exception.Message}";
                }

                lock (s_pendingLock)
                {
                    pendingResult = result;
                    refreshResultReady = true;
                }
            });
        }

        private static void ApplyPendingResultIfAvailable()
        {
            PendingRefreshResult result;

            lock (s_pendingLock)
            {
                result = pendingResult;
                pendingResult = null;
                refreshResultReady = false;
            }

            if (result == null)
            {
                return;
            }

            try
            {
                Snapshot = BuildSnapshot(result);
            }
            catch (ArgumentException exception)
            {
                Snapshot = GitStatusSnapshot.CreateFailed(exception.Message, result.CompletedAt);
            }
            catch (IOException exception)
            {
                Snapshot = GitStatusSnapshot.CreateFailed(exception.Message, result.CompletedAt);
            }
            catch (NotSupportedException exception)
            {
                Snapshot = GitStatusSnapshot.CreateFailed(exception.Message, result.CompletedAt);
            }
            catch (UnauthorizedAccessException exception)
            {
                Snapshot = GitStatusSnapshot.CreateFailed(exception.Message, result.CompletedAt);
            }
            finally
            {
                RepositoryRoot = Snapshot.RepositoryDetected
                    ? result.RunResult?.RepositoryRoot ?? string.Empty
                    : string.Empty;
                IsRefreshInProgress = false;
                RaiseStatusChanged();
            }
        }

        private static void RaiseStatusChanged()
        {
            if (IsAssetImportWorkerProcess())
            {
                return;
            }

            EditorApplication.RepaintProjectWindow();
            InternalEditorUtility.RepaintAllViews();
            StatusChanged?.Invoke();
        }

        #endregion

        #region Snapshot Construction

        private static GitStatusSnapshot BuildSnapshot(PendingRefreshResult pending)
        {
            if (!string.IsNullOrEmpty(pending.Error))
            {
                return GitStatusSnapshot.CreateFailed(pending.Error, pending.CompletedAt);
            }

            var result = pending.RunResult;

            if (result == null)
            {
                return GitStatusSnapshot.CreateFailed("Git status did not return a result.", pending.CompletedAt);
            }

            if (!result.Succeeded)
            {
                return GitStatusSnapshot.CreateFailed(GetFriendlyError(result), pending.CompletedAt);
            }

            var rawEntries = pending.Entries ?? new List<GitStatusEntry>();
            var visibleEntries = new List<GitStatusEntry>();
            var assetStatuses = new Dictionary<string, GitStatusEntry>(GitPathComparer.Comparer);
            var deletedEntries = new List<GitStatusEntry>();
            var outsideAssetsChangedCount = 0;
            var outsideAssetsDeletedCount = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < rawEntries.Count; index++)
            {
                var entry = NormalizeEntryToUnityProject(rawEntries[index], result.ProjectPathInRepository);

                if (entry == null)
                {
                    continue;
                }

                if (!IsAssetsRelatedEntry(entry))
                {
                    if (entry.IsDeleted)
                    {
                        outsideAssetsDeletedCount++;
                    }
                    else
                    {
                        outsideAssetsChangedCount++;
                    }

                    continue;
                }

                visibleEntries.Add(entry);

                if (entry.IsDeleted)
                {
                    AddDeletedEntry(deletedEntries, entry);
                    continue;
                }

                var visiblePath = ResolveVisibleProjectPath(pending.ProjectRoot, entry);

                if (string.IsNullOrEmpty(visiblePath) ||
                    !IsUnderAssets(visiblePath) ||
                    IsExcludedRootPath(visiblePath))
                {
                    continue;
                }

                var isAggregateTarget = entry.IsDeleted ||
                    !GitPathComparer.AreEqual(visiblePath, GetPrimaryVisiblePath(entry));

                AddStatus(assetStatuses, visiblePath, entry.WithDisplayPath(visiblePath, isAggregateTarget));
                AddParentFolderAggregates(assetStatuses, visiblePath, entry);
            }

            var deletedEntriesByFolder = BuildDeletedEntriesByFolder(deletedEntries);

            return new GitStatusSnapshot(
                true,
                result.Branch ?? string.Empty,
                pending.CompletedAt,
                string.Empty,
                outsideAssetsChangedCount,
                outsideAssetsDeletedCount,
                assetStatuses,
                visibleEntries,
                deletedEntries,
                deletedEntriesByFolder);
        }

        private static GitStatusEntry NormalizeEntryToUnityProject(
            GitStatusEntry entry,
            string projectPathInRepository)
        {
            if (entry == null)
                return null;

            var path = NormalizeRepoPathToUnityProjectPath(entry.Path, projectPathInRepository);
            var originalPath = NormalizeRepoPathToUnityProjectPath(entry.OriginalPath, projectPathInRepository);

            return string.IsNullOrEmpty(path) && string.IsNullOrEmpty(originalPath)
                ? null
                : new GitStatusEntry(
                    path,
                    originalPath,
                    entry.Kind,
                    GitStatusEntry.IsMetaPath(path) || GitStatusEntry.IsMetaPath(originalPath),
                    false,
                    path,
                    entry.RepositoryPath,
                    entry.OriginalRepositoryPath);
        }

        private static string NormalizeRepoPathToUnityProjectPath(string repoRelativePath, string projectPathInRepository)
        {
            var path = GitStatusEntry.NormalizePath(repoRelativePath);

            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var projectPrefix = GitStatusEntry.NormalizePath(projectPathInRepository).Trim('/');

            if (string.IsNullOrEmpty(projectPrefix))
                return path;

            if (GitPathComparer.AreEqual(path, projectPrefix))
                return string.Empty;

            var prefixWithSlash = projectPrefix + "/";
            return GitPathComparer.StartsWith(path, prefixWithSlash)
                ? path[prefixWithSlash.Length..]
                : path;
        }

        private static bool IsAssetsRelatedEntry(GitStatusEntry entry) =>
            IsUnderAssets(entry.Path) || IsUnderAssets(entry.OriginalPath);

        #endregion

        #region Error Handling

        private static string GetFriendlyError(GitStatusRunResult result)
        {
            if (result.TimedOut)
            {
                return "Git status timed out after " +
                    Constants.GitCommandTimeoutMilliseconds / 1000 +
                    " seconds.";
            }

            var stderr = result.StatusError ?? string.Empty;

            if (stderr.IndexOf("not a git repository", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "No Git repository found at or above the Unity project root.";
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                return "Git process could not be started: " + result.Error;
            }

            if (!string.IsNullOrEmpty(stderr))
            {
                return FirstLine(stderr);
            }

            return "Git status failed with exit code " + result.StatusExitCode + ".";
        }

        private static string FirstLine(string text)
        {
            var normalized = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            var lineBreak = normalized.IndexOf('\n');
            return lineBreak >= 0 ? normalized[..lineBreak] : normalized;
        }

        #endregion

        #region Visible Path Mapping

        private static void AddParentFolderAggregates(
            Dictionary<string, GitStatusEntry> assetStatuses,
            string visiblePath,
            GitStatusEntry sourceEntry)
        {
            var parent = GetParentPath(visiblePath);

            while (IsUnderAssets(parent) && !IsExcludedRootPath(parent))
            {
                AddStatus(assetStatuses, parent, sourceEntry.WithDisplayPath(parent, true));
                parent = GetParentPath(parent);
            }
        }

        private static void AddStatus(
            Dictionary<string, GitStatusEntry> assetStatuses,
            string assetPath,
            GitStatusEntry entry)
        {
            if (string.IsNullOrEmpty(assetPath) || IsExcludedRootPath(assetPath))
                return;

            if (!assetStatuses.TryGetValue(assetPath, out var existing) ||
                ShouldReplaceVisibleEntry(entry, existing))
            {
                assetStatuses[assetPath] = entry;
            }
        }

        private static bool ShouldReplaceVisibleEntry(GitStatusEntry candidate, GitStatusEntry current) =>
            !ShouldKeepCurrentNonMetaVisibleEntry(candidate, current) &&
            (ShouldPreferNonMetaVisibleEntry(candidate, current) ||
                   GitStatusDescriptors.HasHigherPriority(candidate.Kind, current.Kind) ||
                   (candidate.Kind == current.Kind && current.IsFolderAggregate && !candidate.IsFolderAggregate));

        private static bool ShouldPreferNonMetaVisibleEntry(GitStatusEntry candidate, GitStatusEntry current) =>
            !candidate.IsFolderAggregate &&
            !current.IsFolderAggregate &&
            !candidate.IsMeta &&
            current.IsMeta;

        private static bool ShouldKeepCurrentNonMetaVisibleEntry(GitStatusEntry candidate, GitStatusEntry current) =>
            !candidate.IsFolderAggregate &&
            !current.IsFolderAggregate &&
            candidate.IsMeta &&
            !current.IsMeta;

        private static void AddDeletedEntry(
            List<GitStatusEntry> deletedEntries,
            GitStatusEntry entry)
        {
            var deletedVisiblePath = GetDeletedVisiblePath(entry);

            if (string.IsNullOrEmpty(deletedVisiblePath) ||
                !IsUnderAssets(deletedVisiblePath) ||
                IsExcludedRootPath(deletedVisiblePath))
                return;

            deletedEntries.Add(entry.WithDisplayPath(deletedVisiblePath, false));
        }

        private static Dictionary<string, IReadOnlyList<GitStatusEntry>> BuildDeletedEntriesByFolder(
            IReadOnlyList<GitStatusEntry> deletedEntries)
        {
            var deletedEntriesByFolder = new Dictionary<string, List<GitStatusEntry>>(GitPathComparer.Comparer);

            foreach (var deletedEntry in deletedEntries)
            {
                var folderPath = GetParentPath(deletedEntry.DisplayPath);

                if (string.IsNullOrEmpty(folderPath) || !IsUnderAssets(folderPath))
                {
                    continue;
                }

                if (!deletedEntriesByFolder.TryGetValue(folderPath, out var folderEntries))
                {
                    folderEntries = new List<GitStatusEntry>();
                    deletedEntriesByFolder[folderPath] = folderEntries;
                }

                folderEntries.Add(deletedEntry);
            }

            var result = new Dictionary<string, IReadOnlyList<GitStatusEntry>>(GitPathComparer.Comparer);

            foreach (var pair in deletedEntriesByFolder)
            {
                pair.Value.Sort((left, right) => GitPathComparer.Compare(left.DisplayPath, right.DisplayPath));
                result[pair.Key] = pair.Value;
            }

            return result;
        }

        private static string ResolveVisibleProjectPath(string projectRoot, GitStatusEntry entry)
        {
            if (entry.Kind is GitStatusKind.Renamed or GitStatusKind.Copied)
            {
                if (IsUnderAssets(entry.Path))
                {
                    return ResolveNonDeletedVisiblePath(projectRoot, entry.Path);
                }

                if (entry.Kind == GitStatusKind.Renamed && IsUnderAssets(entry.OriginalPath))
                {
                    return FindNearestExistingParentFolder(projectRoot, entry.OriginalPath);
                }

                return string.Empty;
            }

            return !IsUnderAssets(entry.Path)
                ? string.Empty
                : entry.Kind == GitStatusKind.Deleted
                    ? FindNearestExistingParentFolder(projectRoot, GetPrimaryVisiblePath(entry))
                    : ResolveNonDeletedVisiblePath(projectRoot, entry.Path);
        }

        private static string ResolveNonDeletedVisiblePath(string projectRoot, string path)
        {
            var visiblePath = GetPrimaryVisiblePath(path);
            return ProjectPathExists(projectRoot, visiblePath)
                ? visiblePath
                : FindNearestExistingParentFolder(projectRoot, visiblePath);
        }

        private static string GetPrimaryVisiblePath(GitStatusEntry entry) => GetPrimaryVisiblePath(entry.Path);

        private static string GetDeletedVisiblePath(GitStatusEntry entry) =>
            IsUnderAssets(entry.Path)
                ? GitStatusEntry.NormalizePath(entry.Path)
                : IsUnderAssets(entry.OriginalPath)
                    ? GitStatusEntry.NormalizePath(entry.OriginalPath)
                    : string.Empty;

        private static string GetPrimaryVisiblePath(string path)
        {
            var normalized = GitStatusEntry.NormalizePath(path);
            return GitStatusEntry.IsMetaPath(normalized)
                ? normalized[..^".meta".Length]
                : normalized;
        }

        private static string FindNearestExistingParentFolder(string projectRoot, string assetPath)
        {
            var current = GitStatusEntry.NormalizePath(assetPath);

            if (!IsUnderAssets(current))
                return string.Empty;

            if (!Directory.Exists(GetFullPath(projectRoot, current)))
            {
                current = GetParentPath(current);
            }

            while (IsUnderAssets(current))
            {
                if (Directory.Exists(GetFullPath(projectRoot, current)))
                {
                    return current;
                }

                current = GetParentPath(current);
            }

            return Directory.Exists(GetFullPath(projectRoot, Constants.AssetsRoot))
                ? Constants.AssetsRoot
                : string.Empty;
        }

        #endregion

        #region Path Helpers

        private static bool ProjectPathExists(string projectRoot, string assetPath)
        {
            var fullPath = GetFullPath(projectRoot, assetPath);
            return File.Exists(fullPath) || Directory.Exists(fullPath);
        }

        private static string GetFullPath(string projectRoot, string assetPath) =>
            Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));

        private static bool IsUnderAssets(string path)
        {
            var normalized = GitStatusEntry.NormalizePath(path);
            return string.Equals(normalized, Constants.AssetsRoot, StringComparison.Ordinal) ||
                normalized.StartsWith(Constants.AssetsRootWithSeparator, StringComparison.Ordinal);
        }

        private static bool IsExcludedRootPath(string path)
        {
            var normalized = GitStatusEntry.NormalizePath(path);
            return string.Equals(normalized, Constants.AssetsRoot, StringComparison.Ordinal) ||
                string.Equals(normalized, Constants.PackagesRoot, StringComparison.Ordinal);
        }

        private static string GetParentPath(string path)
        {
            var normalized = GitStatusEntry.NormalizePath(path);
            var slashIndex = normalized.LastIndexOf('/');
            return slashIndex > 0 ? normalized[..slashIndex] : string.Empty;
        }

        private static string GetUnityProjectRoot()
        {
            var assetsPath = Application.dataPath;

            if (string.IsNullOrEmpty(assetsPath))
            {
                return string.Empty;
            }

            var parent = Directory.GetParent(assetsPath);
            return parent != null ? parent.FullName : string.Empty;
        }

        private static bool IsAssetImportWorkerProcess() => AssetDatabase.IsAssetImportWorkerProcess();

        private static void ScheduleEventDrivenRefresh(string reason)
        {
            if (UnityGitStatusSettings.RefreshMode == GitRefreshMode.EventDriven)
                ScheduleRefresh(reason);
        }

        #endregion

        #region Nested Types

        private sealed class PendingRefreshResult
        {
            /// <summary>Project root associated with the completed background refresh.</summary>
            public string ProjectRoot;

            /// <summary>Timestamp captured when the background refresh completed.</summary>
            public DateTime CompletedAt;

            /// <summary>Raw Git command result returned by the background runner.</summary>
            public GitStatusRunResult RunResult;

            /// <summary>Parsed entries produced from the raw Git status output.</summary>
            public List<GitStatusEntry> Entries;

            /// <summary>User-facing error message associated with the pending result, if any.</summary>
            public string Error;
        }

        #endregion
    }
}
