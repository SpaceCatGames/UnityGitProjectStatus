using System;
using System.Linq;
using UnityEngine;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Describes one normalized Git status used by parsing and rendering.
    /// The descriptor keeps marker content, colors, priority, and porcelain patterns together.
    /// This provides one source of truth for each status kind.
    /// </summary>
    internal sealed class GitStatusDescriptor
    {
        #region Properties

        /// <summary>Status kind represented by the descriptor.</summary>
        internal GitStatusKind Kind { get; }

        /// <summary>Priority used when several statuses compete for the same visible path.</summary>
        internal int Priority { get; }

        /// <summary>Default marker content used when Calc Mode is disabled.</summary>
        internal GUIContent DefaultContent { get; }

        /// <summary>Alternative marker content used when Calc Mode is enabled.</summary>
        internal GUIContent CalcContent { get; }

        /// <summary>Background color used for the status badge.</summary>
        internal Color BadgeColor { get; }

        /// <summary>Text color used for the status marker.</summary>
        internal Color TextColor { get; }

        /// <summary>Two-character porcelain patterns matched by the descriptor.</summary>
        internal string[] PorcelainPatterns { get; }

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new descriptor for one Git status kind.
        /// Parsing patterns are two-character porcelain patterns where `*` means "any character".
        /// Marker content, badge color, and text color are stored directly in the descriptor.
        /// </summary>
        /// <param name="kind">Status kind represented by the descriptor.</param>
        /// <param name="priority">Priority used when collapsing multiple statuses onto one visible path.</param>
        /// <param name="defaultMarker">Default letter marker shown when Calc Mode is disabled.</param>
        /// <param name="calcMarker">Alternative marker shown when Calc Mode is enabled.</param>
        /// <param name="tooltip">Tooltip text shared by both marker variants.</param>
        /// <param name="badgeColor">Background color used for the status badge.</param>
        /// <param name="textColor">Text color used for the status marker.</param>
        /// <param name="porcelainPatterns">Supported porcelain patterns for this status descriptor.</param>
        internal GitStatusDescriptor(
            GitStatusKind kind,
            int priority,
            string defaultMarker,
            string calcMarker,
            string tooltip,
            Color badgeColor,
            Color textColor,
            params string[] porcelainPatterns)
        {
            Kind = kind;
            Priority = priority;
            DefaultContent = new GUIContent(defaultMarker ?? string.Empty, tooltip ?? string.Empty);
            CalcContent = new GUIContent(calcMarker ?? string.Empty, tooltip ?? string.Empty);
            BadgeColor = badgeColor;
            TextColor = textColor;
            PorcelainPatterns = porcelainPatterns ?? Array.Empty<string>();
        }

        #endregion

        #region Matching

        /// <summary>
        /// Determines whether a porcelain status pair matches this descriptor.
        /// Pattern characters compare literally unless the pattern character is `*`.
        /// Invalid patterns are ignored.
        /// </summary>
        /// <param name="x">Index status character from porcelain output.</param>
        /// <param name="y">Worktree status character from porcelain output.</param>
        /// <returns>True when one of the configured patterns matches the provided pair; otherwise false.</returns>
        internal bool Matches(char x, char y) => PorcelainPatterns.Any(pattern => IsPatternMatch(pattern, x, y));

        private static bool IsPatternMatch(string pattern, char x, char y) =>
            !string.IsNullOrEmpty(pattern) && pattern.Length == 2 && MatchesPatternChar(pattern[0], x) &&
            MatchesPatternChar(pattern[1], y);

        private static bool MatchesPatternChar(char patternChar, char value) => patternChar == '*' || patternChar == value;

        #endregion
    }
}
