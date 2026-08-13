using System.Collections.Generic;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Parses the null-delimited output of `git status --porcelain=v1 -z`.
    /// The parser converts Git records into immutable package entries and normalizes rename or copy records.
    /// It intentionally ignores unsupported or empty records so the editor UI only sees meaningful statuses.
    /// </summary>
    internal static class GitStatusParser
    {
        /// <summary>
        /// Parses Git porcelain output into immutable package entries.
        /// The method expects null-delimited porcelain v1 output produced with the `-z` switch.
        /// Unsupported or empty records are skipped.
        /// </summary>
        /// <param name="porcelainOutput">Raw porcelain output returned by Git.</param>
        /// <returns>Parsed immutable status entries ready for snapshot normalization.</returns>
        internal static List<GitStatusEntry> Parse(string porcelainOutput)
        {
            var entries = new List<GitStatusEntry>();

            if (string.IsNullOrEmpty(porcelainOutput))
            {
                return entries;
            }

            var records = porcelainOutput.Split('\0');

            for (var index = 0; index < records.Length; index++)
            {
                var record = records[index];

                if (string.IsNullOrEmpty(record) || record.Length < 4 || record[2] != ' ')
                {
                    continue;
                }

                var x = record[0];
                var y = record[1];
                if (!GitStatusDescriptors.TryGetByPorcelainStatus(x, y, out var descriptor))
                {
                    continue;
                }

                var kind = descriptor.Kind;

                var path = GitStatusEntry.NormalizePath(record[3..]);
                var originalPath = string.Empty;

                if (kind is GitStatusKind.Renamed or GitStatusKind.Copied && index + 1 < records.Length)
                {
                    // In porcelain v1 -z, rename and copy records emit the new path first and the source path next.
                    originalPath = GitStatusEntry.NormalizePath(records[++index]);
                }

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                entries.Add(new GitStatusEntry(
                    path,
                    originalPath,
                    kind,
                    GitStatusEntry.IsMetaPath(path),
                    false,
                    path));
            }

            return entries;
        }
    }
}
