using System.Globalization;
using System.Linq;
using System.Text;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Builds an applicable partial patch from selected unified diff lines.
    /// </summary>
    internal static class GitPatchBuilder
    {
        #region Public API

        /// <summary>
        /// Builds a patch containing the selected changed lines and required context.
        /// </summary>
        /// <param name="fileDiff">The parsed source diff.</param>
        /// <returns>An empty string when nothing is selected; otherwise an applicable patch.</returns>
        internal static string BuildSelectedPatch(GitFileDiff fileDiff)
        {
            if (fileDiff == null || fileDiff.RequiresWholeFileAction || !fileDiff.HasSelection) return string.Empty;

            var selectableLines = fileDiff.Hunks
                .SelectMany(hunk => hunk.Lines)
                .Where(line => line.IsSelectable)
                .ToList();

            if (selectableLines.Count > 0 && selectableLines.All(line => line.IsSelected))
            {
                return fileDiff.OriginalPatch;
            }

            var builder = new StringBuilder();
            foreach (var header in fileDiff.HeaderLines) builder.Append(header).Append('\n');

            var selectedLineDelta = 0;

            foreach (var hunk in fileDiff.Hunks.Where(item => item.Lines.Any(line => line.IsSelectable && line.IsSelected)))
            {
                AppendSelectedGroups(builder, hunk, ref selectedLineDelta);
            }

            return builder.ToString();
        }

        #endregion

        #region Helpers

        private static void AppendSelectedGroups(StringBuilder builder, GitDiffHunk hunk, ref int selectedLineDelta)
        {
            var oldCursor = hunk.OldStart;
            var index = 0;

            while (index < hunk.Lines.Count)
            {
                var line = hunk.Lines[index];

                if (!line.IsSelectable || !line.IsSelected)
                {
                    if (line.Kind is GitDiffLineKind.Context or GitDiffLineKind.Removed) oldCursor++;
                    index++;
                    continue;
                }

                AppendSelectedGroup(builder, hunk, ref index, ref oldCursor, ref selectedLineDelta);
            }
        }

        private static void AppendSelectedGroup(
            StringBuilder builder,
            GitDiffHunk hunk,
            ref int index,
            ref int oldCursor,
            ref int selectedLineDelta)
        {
            var groupStartOldCursor = oldCursor;
            var oldCount = 0;
            var newCount = 0;
            var output = new StringBuilder();

            while (index < hunk.Lines.Count)
            {
                var line = hunk.Lines[index];
                if (!line.IsSelectable || !line.IsSelected) break;

                if (line.Kind == GitDiffLineKind.Removed)
                {
                    output.Append('-').Append(line.Content).Append('\n');
                    oldCount++;
                    oldCursor++;
                }
                else
                {
                    output.Append('+').Append(line.Content).Append('\n');
                    newCount++;
                }

                index++;
            }

            if (index < hunk.Lines.Count && hunk.Lines[index].Kind == GitDiffLineKind.Metadata)
            {
                output.Append(hunk.Lines[index].Content).Append('\n');
                index++;
            }

            var oldStart = oldCount == 0 ? groupStartOldCursor - 1 : groupStartOldCursor;
            var partialNewCursor = groupStartOldCursor + selectedLineDelta;
            var newStart = newCount == 0 ? partialNewCursor - 1 : partialNewCursor;

            builder.Append("@@ -")
                .Append(FormatRange(oldStart, oldCount))
                .Append(" +")
                .Append(FormatRange(newStart, newCount))
                .Append(" @@\n")
                .Append(output);
            selectedLineDelta += newCount - oldCount;
        }

        private static string FormatRange(int start, int count) =>
            start.ToString(CultureInfo.InvariantCulture) + "," + count.ToString(CultureInfo.InvariantCulture);

        #endregion
    }
}
