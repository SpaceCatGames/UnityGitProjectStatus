using System.Collections.Generic;
using UnityEngine;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Central registry of status descriptors used by parsing and rendering code.
    /// The registry keeps priorities, parser mappings, and marker variants in one place.
    /// This removes duplicated status metadata from parser and UI helper classes.
    /// </summary>
    internal static class GitStatusDescriptors
    {
        #region Fields

        private static readonly GitStatusDescriptor s_none = new(
            GitStatusKind.None,
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            new Color(0f, 0f, 0f, 0f),
            Color.white);

        private static readonly GitStatusDescriptor s_conflicted = new(
            GitStatusKind.Conflicted,
            8,
            "!",
            "!",
            "Conflicted",
            new Color(1f, 0f, 0.08f, 1f),
            Color.white,
            "U*",
            "*U",
            "DD",
            "AA");

        private static readonly GitStatusDescriptor s_untracked = new(
            GitStatusKind.Untracked,
            2,
            "?",
            "?",
            "Untracked",
            new Color(0.46f, 0.64f, 0.76f, 1f),
            Color.black,
            "??");

        private static readonly GitStatusDescriptor s_ignored = new(
            GitStatusKind.Ignored,
            1,
            "I",
            "X",
            "Ignored",
            new Color(0.42f, 0.42f, 0.42f, 1f),
            Color.white,
            "!!");

        private static readonly GitStatusDescriptor s_renamed = new(
            GitStatusKind.Renamed,
            6,
            "R",
            "±",
            "Renamed",
            new Color(0.62f, 0.18f, 0.82f, 1f),
            Color.white,
            "R*",
            "*R");

        private static readonly GitStatusDescriptor s_copied = new(
            GitStatusKind.Copied,
            3,
            "C",
            "/",
            "Copied",
            new Color(0.02f, 0.62f, 0.78f, 1f),
            Color.black,
            "C*",
            "*C");

        private static readonly GitStatusDescriptor s_deleted = new(
            GitStatusKind.Deleted,
            7,
            "D",
            "-",
            "Deleted",
            new Color(0.86f, 0.12f, 0.12f, 1f),
            Color.white,
            "D*",
            "*D");

        private static readonly GitStatusDescriptor s_added = new(
            GitStatusKind.Added,
            5,
            "A",
            "+",
            "Added",
            new Color(0.12f, 0.58f, 0.24f, 1f),
            Color.white,
            "A*",
            "*A");

        private static readonly GitStatusDescriptor s_modified = new(
            GitStatusKind.Modified,
            4,
            "M",
            "*",
            "Modified",
            new Color(0.95f, 0.58f, 0.14f, 1f),
            Color.black,
            "M*",
            "*M",
            "T*",
            "*T");

        private static readonly GitStatusDescriptor[] s_parseOrder =
        {
            s_conflicted,
            s_untracked,
            s_ignored,
            s_renamed,
            s_copied,
            s_deleted,
            s_added,
            s_modified
        };

        private static readonly Dictionary<GitStatusKind, GitStatusDescriptor> s_descriptorsByKind = new()
        {
            [GitStatusKind.None] = s_none,
            [GitStatusKind.Modified] = s_modified,
            [GitStatusKind.Added] = s_added,
            [GitStatusKind.Deleted] = s_deleted,
            [GitStatusKind.Renamed] = s_renamed,
            [GitStatusKind.Copied] = s_copied,
            [GitStatusKind.Untracked] = s_untracked,
            [GitStatusKind.Conflicted] = s_conflicted,
            [GitStatusKind.Ignored] = s_ignored
        };

        #endregion

        #region Lookup

        /// <summary>
        /// Attempts to resolve a status descriptor from a porcelain status pair.
        /// Descriptors are checked in a deliberate order so special cases win before broader wildcard matches.
        /// Unknown pairs return false and no descriptor.
        /// </summary>
        /// <param name="x">Index status character from porcelain output.</param>
        /// <param name="y">Worktree status character from porcelain output.</param>
        /// <param name="descriptor">Resolved descriptor when a match is found.</param>
        /// <returns>True when a descriptor matches the porcelain pair; otherwise false.</returns>
        internal static bool TryGetByPorcelainStatus(char x, char y, out GitStatusDescriptor descriptor)
        {
            foreach (var candidate in s_parseOrder)
            {
                if (!candidate.Matches(x, y)) continue;
                descriptor = candidate;
                return true;
            }

            descriptor = s_none;
            return false;
        }

        /// <summary>
        /// Resolves the descriptor registered for a status kind.
        /// Unknown kinds fall back to the empty descriptor.
        /// This keeps callers simple when they only need content or priority metadata.
        /// </summary>
        /// <param name="kind">Status kind whose descriptor should be returned.</param>
        /// <returns>Descriptor registered for the requested kind, or the empty descriptor.</returns>
        internal static GitStatusDescriptor Get(GitStatusKind kind) =>
            s_descriptorsByKind.GetValueOrDefault(kind, s_none);

        /// <summary>
        /// Determines whether one status kind has a higher collapse priority than another.
        /// Priority values come from the registered descriptors instead of UI helper code.
        /// Equal priority values do not count as higher priority.
        /// </summary>
        /// <param name="candidate">Status that might replace the current one.</param>
        /// <param name="current">Status currently associated with the visible path.</param>
        /// <returns>True when the candidate has a strictly higher priority; otherwise false.</returns>
        internal static bool HasHigherPriority(GitStatusKind candidate, GitStatusKind current) =>
            Get(candidate).Priority > Get(current).Priority;

        #endregion
    }
}
