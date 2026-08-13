using System;
using System.Collections.Generic;
using System.Globalization;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Parses one-file unified patches produced by Git.
    /// </summary>
    internal static class GitDiffParser
    {
        #region Parsing

        /// <summary>
        /// Parses a unified diff.
        /// </summary>
        /// <param name="path">The repository-relative file path.</param>
        /// <param name="side">The represented repository side.</param>
        /// <param name="patch">The raw unified patch.</param>
        /// <returns>The parsed file diff.</returns>
        internal static GitFileDiff Parse(string path, GitDiffSide side, string patch)
        {
            var lines = NormalizeLines(patch);
            var headers = new List<string>();
            var hunks = new List<GitDiffHunk>();
            var isBinary = false;
            var isNewFile = false;
            var isDeletedFile = false;
            var isRenameOrCopy = false;
            var isConflicted = Array.Exists(
                lines,
                line => line.StartsWith("@@@", StringComparison.Ordinal));
            var index = 0;

            while (index < lines.Length && !lines[index].StartsWith("@@", StringComparison.Ordinal))
            {
                var line = lines[index++];
                if (line.Length == 0 && index == lines.Length) continue;
                headers.Add(line);
                isBinary |= line.StartsWith("Binary files ", StringComparison.Ordinal) || line == "GIT binary patch";
                isNewFile |= line.StartsWith("new file mode ", StringComparison.Ordinal);
                isDeletedFile |= line.StartsWith("deleted file mode ", StringComparison.Ordinal);
                isRenameOrCopy |= line.StartsWith("rename from ", StringComparison.Ordinal) ||
                                  line.StartsWith("rename to ", StringComparison.Ordinal) ||
                                  line.StartsWith("copy from ", StringComparison.Ordinal) ||
                                  line.StartsWith("copy to ", StringComparison.Ordinal);
            }

            while (!isConflicted && index < lines.Length)
            {
                if (!lines[index].StartsWith("@@", StringComparison.Ordinal))
                {
                    index++;
                    continue;
                }

                var header = lines[index++];
                ParseHeader(header, out var oldStart, out var newStart);
                var oldLine = oldStart;
                var newLine = newStart;
                var hunkLines = new List<GitDiffLine>();

                while (index < lines.Length && !lines[index].StartsWith("@@", StringComparison.Ordinal))
                {
                    var raw = lines[index++];
                    if (raw.Length == 0 && index == lines.Length) break;
                    var prefix = raw.Length == 0 ? ' ' : raw[0];
                    var content = raw.Length == 0 ? string.Empty : raw[1..];

                    switch (prefix)
                    {
                        case '+':
                            hunkLines.Add(new GitDiffLine(GitDiffLineKind.Added, content, 0, newLine++));
                            break;
                        case '-':
                            hunkLines.Add(new GitDiffLine(GitDiffLineKind.Removed, content, oldLine++, 0));
                            break;
                        case '\\':
                            hunkLines.Add(new GitDiffLine(GitDiffLineKind.Metadata, raw, 0, 0));
                            break;
                        default:
                            hunkLines.Add(new GitDiffLine(GitDiffLineKind.Context, content, oldLine++, newLine++));
                            break;
                    }
                }

                hunks.Add(new GitDiffHunk(header, oldStart, newStart, hunkLines));
            }

            return new GitFileDiff(
                path,
                side,
                headers,
                hunks,
                isBinary,
                isNewFile,
                isDeletedFile,
                isRenameOrCopy,
                isConflicted)
            {
                OriginalPatch = patch ?? string.Empty
            };
        }

        #endregion

        #region Helpers

        private static string[] NormalizeLines(string patch) =>
            (patch ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        private static void ParseHeader(string header, out int oldStart, out int newStart)
        {
            oldStart = ParseRangeStart(header, '-');
            newStart = ParseRangeStart(header, '+');
        }

        private static int ParseRangeStart(string header, char marker)
        {
            var markerIndex = header.IndexOf(marker);
            if (markerIndex < 0) return 0;
            var end = header.IndexOfAny(new[] { ',', ' ' }, markerIndex + 1);
            var value = end < 0 ? header[(markerIndex + 1)..] : header.Substring(markerIndex + 1, end - markerIndex - 1);
            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        #endregion
    }
}
