using System.Collections.Generic;
using System.Linq;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Orders Git status entries for display in the Git Status window.
    /// </summary>
    internal static class GitStatusEntrySorter
    {
        /// <summary>
        /// Materializes entries in the requested display order.
        /// </summary>
        /// <param name="entries">Entries to sort.</param>
        /// <param name="sortMode">Requested ordering mode.</param>
        /// <param name="preferDisplayPath">Whether remapped Unity display paths should be preferred.</param>
        /// <returns>A new list containing the sorted entries.</returns>
        internal static List<GitStatusEntry> Sort(
            IEnumerable<GitStatusEntry> entries,
            GitStatusSortMode sortMode,
            bool preferDisplayPath)
        {
            var source = entries ?? Enumerable.Empty<GitStatusEntry>();
            return sortMode switch
            {
                GitStatusSortMode.PathDescending => source
                    .OrderByDescending(entry => GetPath(entry, preferDisplayPath), GitPathComparer.Comparer)
                    .ToList(),
                GitStatusSortMode.FileNameAscending => source
                    .OrderBy(entry => GetFileName(entry, preferDisplayPath), GitPathComparer.Comparer)
                    .ThenBy(entry => GetPath(entry, preferDisplayPath), GitPathComparer.Comparer)
                    .ToList(),
                GitStatusSortMode.FileNameDescending => source
                    .OrderByDescending(entry => GetFileName(entry, preferDisplayPath), GitPathComparer.Comparer)
                    .ThenByDescending(entry => GetPath(entry, preferDisplayPath), GitPathComparer.Comparer)
                    .ToList(),
                GitStatusSortMode.FileStatus => source
                    .OrderBy(entry => GetStatusOrder(entry.Kind))
                    .ThenBy(entry => GetPath(entry, preferDisplayPath), GitPathComparer.Comparer)
                    .ToList(),
                _ => source
                    .OrderBy(entry => GetPath(entry, preferDisplayPath), GitPathComparer.Comparer)
                    .ToList()
            };
        }

        private static string GetPath(GitStatusEntry entry, bool preferDisplayPath) =>
            preferDisplayPath && !string.IsNullOrEmpty(entry.DisplayPath)
                ? entry.DisplayPath
                : entry.Path;

        private static string GetFileName(GitStatusEntry entry, bool preferDisplayPath)
        {
            var path = GetPath(entry, preferDisplayPath);
            var slashIndex = path.LastIndexOf('/');
            return slashIndex >= 0 ? path[(slashIndex + 1)..] : path;
        }

        private static int GetStatusOrder(GitStatusKind kind) => kind switch
        {
            GitStatusKind.Conflicted => 0,
            GitStatusKind.Deleted => 1,
            GitStatusKind.Renamed => 2,
            GitStatusKind.Added => 3,
            GitStatusKind.Modified => 4,
            GitStatusKind.Copied => 5,
            GitStatusKind.Untracked => 6,
            GitStatusKind.Ignored => 7,
            _ => 8
        };
    }
}
