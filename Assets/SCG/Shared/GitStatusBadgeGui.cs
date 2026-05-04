using UnityEditor;
using UnityEngine;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Draws compact badge visuals shared by Project, Inspector, and the status window.
    /// The helper keeps badge colors, content selection, and text styling consistent across surfaces.
    /// </summary>
    internal static class GitStatusBadgeGui
    {
        private static GUIStyle s_badgeStyle;

        /// <summary>
        /// Draws one Git badge using the shared content offset behavior for compact window-style badges.
        /// Calc Mode markers receive the standard offset used by the status window and Inspector header badge.
        /// Non-Calc content is rendered without any additional shift.
        /// </summary>
        /// <param name="badgeRect">Target rect that should contain the badge.</param>
        /// <param name="descriptor">Resolved status descriptor that defines content and colors.</param>
        /// <param name="calcMode">Whether symbolic Calc Mode marker content should be used.</param>
        internal static void Draw(Rect badgeRect, GitStatusDescriptor descriptor, bool calcMode) =>
            Draw(badgeRect, descriptor, calcMode, GitStatusBadgeLayout.GetCompactBadgeContentOffset(calcMode));

        /// <summary>
        /// Draws one Git badge inside the requested rect.
        /// Content selection respects Calc Mode and styling is applied directly to a transient GUIStyle instance.
        /// The method assumes descriptor metadata has already been resolved by the caller.
        /// </summary>
        /// <param name="badgeRect">Target rect that should contain the badge.</param>
        /// <param name="descriptor">Resolved status descriptor that defines content and colors.</param>
        /// <param name="calcMode">Whether symbolic Calc Mode marker content should be used.</param>
        /// <param name="contentOffset">Optional offset applied when drawing marker content inside the badge.</param>
        internal static void Draw(Rect badgeRect, GitStatusDescriptor descriptor, bool calcMode, Vector2 contentOffset) =>
            Draw(badgeRect, descriptor, GetBadgeContent(descriptor, calcMode), contentOffset);

        private static void Draw(Rect badgeRect, GitStatusDescriptor descriptor, GUIContent content, Vector2 contentOffset)
        {
            EditorGUI.DrawRect(badgeRect, descriptor.BadgeColor);
            var style = GetBadgeStyle(badgeRect.height, descriptor.TextColor, contentOffset);
            GUI.Label(badgeRect, content, style);
        }

        /// <summary>
        /// Applies one text color to all GUIStyle text states used by badge labels.
        /// This keeps hover, active, focused, and normal states visually identical for the compact badge style.
        /// The method mutates the provided style instance directly.
        /// </summary>
        /// <param name="style">GUIStyle whose text colors should be updated.</param>
        /// <param name="color">Text color that should be applied to all style states.</param>
        internal static void ApplyTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            style.onNormal.textColor = color;
            style.onHover.textColor = color;
            style.onActive.textColor = color;
            style.onFocused.textColor = color;
        }

        private static GUIStyle GetBadgeStyle(float badgeHeight, Color textColor, Vector2 contentOffset)
        {
            s_badgeStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };

            s_badgeStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(badgeHeight - 1f, 7f, 10f));
            s_badgeStyle.contentOffset = contentOffset;
            ApplyTextColor(s_badgeStyle, textColor);
            return s_badgeStyle;
        }

        private static GUIContent GetBadgeContent(GitStatusDescriptor descriptor, bool calcMode) =>
            calcMode ? descriptor.CalcContent : descriptor.DefaultContent;
    }
}
