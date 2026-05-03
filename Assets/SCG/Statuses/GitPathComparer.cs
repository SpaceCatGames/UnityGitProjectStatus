using System;
using System.IO;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Provides path comparison helpers that follow the current editor platform file system semantics.
    /// Windows path identity is treated as case-insensitive, while Linux and macOS preserve case-sensitive comparisons.
    /// The helper is intended for path identity and ordering logic, not for general text search.
    /// </summary>
    internal static class GitPathComparer
    {
        #region Fields

        private static readonly bool s_useCaseInsensitiveComparison = Path.DirectorySeparatorChar == '\\';

        #endregion

        #region Properties

        /// <summary>Comparer used when normalized paths act as dictionary keys.</summary>
        internal static StringComparer Comparer =>
            s_useCaseInsensitiveComparison ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        /// <summary>String comparison used when normalized paths are compared directly.</summary>
        internal static StringComparison Comparison =>
            s_useCaseInsensitiveComparison ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        #endregion

        #region Helpers

        /// <summary>Determines whether two normalized paths should be treated as the same path on the current platform.</summary>
        /// <param name="left">First normalized path.</param>
        /// <param name="right">Second normalized path.</param>
        /// <returns>True when both values represent the same path under the current platform path semantics.</returns>
        internal static bool AreEqual(string left, string right) => string.Equals(left, right, Comparison);

        /// <summary>Determines whether a normalized path starts with a normalized prefix under the current platform path semantics.</summary>
        /// <param name="path">Normalized path that should be tested.</param>
        /// <param name="prefix">Normalized prefix that should match the path start.</param>
        /// <returns>True when the path starts with the requested prefix; otherwise false.</returns>
        internal static bool StartsWith(string path, string prefix) =>
            !string.IsNullOrEmpty(path) &&
            !string.IsNullOrEmpty(prefix) &&
            path.StartsWith(prefix, Comparison);

        /// <summary>Compares two normalized paths using the current platform path semantics.</summary>
        /// <param name="left">First normalized path.</param>
        /// <param name="right">Second normalized path.</param>
        /// <returns>A signed comparison result compatible with standard string sorting.</returns>
        internal static int Compare(string left, string right) => string.Compare(left, right, Comparison);

        #endregion
    }
}
