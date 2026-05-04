using UnityEditor;
using UnityEngine;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Provides shared badge layout helpers for editor rendering.
    /// The class keeps offset and rect calculations in one place.
    /// It is used by the badge drawing code across supported editor surfaces.
    /// </summary>
    internal static class GitStatusBadgeLayout
    {
        /// <summary>
        /// Returns the content offset used inside compact badges.
        /// Calc Mode uses an adjusted horizontal placement for compact marker content.
        /// Non-Calc Mode badges keep the default centered placement.
        /// </summary>
        /// <param name="calcMode">Whether the compact badge is rendering Calc Mode content.</param>
        /// <returns>Offset that should be applied to the compact badge content.</returns>
        internal static Vector2 GetCompactBadgeContentOffset(bool calcMode) =>
            calcMode
                ? new Vector2(Constants.CompactBadgeCalcModeContentOffsetX, 0f)
                : Vector2.zero;

        /// <summary>
        /// Returns the content offset used inside Project window badges.
        /// Right-aligned badges keep the default centered content placement.
        /// Icon-corner badges use a dedicated offset tuned for the compact overlay layout.
        /// </summary>
        /// <param name="rightAlignedBadges">Whether the Project window is using the right-aligned badge layout.</param>
        /// <returns>Offset that should be applied to the Project badge content.</returns>
        internal static Vector2 GetProjectBadgeContentOffset(bool rightAlignedBadges) => rightAlignedBadges
                ? Vector2.zero
                : new Vector2(Constants.OverlayIconCornerBadgeContentOffsetX, Constants.OverlayIconCornerBadgeContentOffsetY);

        /// <summary>
        /// Attempts to reconstruct the full Inspector header rect after Unity finishes drawing the default header.
        /// Unity exposes only the remaining content rect at this stage, so the original header bounds are reconstructed from the surrounding layout data.
        /// The result is used to place the Inspector badge against the visible header area.
        /// </summary>
        /// <param name="postHeaderContentRect">Layout rect produced after Unity draws the default Inspector header.</param>
        /// <param name="headerRect">Resolved Inspector header rect when reconstruction succeeds.</param>
        /// <returns>True when the Inspector header rect was reconstructed successfully; otherwise false.</returns>
        internal static bool TryGetInspectorHeaderRect(Rect postHeaderContentRect, out Rect headerRect)
        {
            if (postHeaderContentRect.width <= 0f)
            {
                headerRect = default;
                return false;
            }

            var postHeaderStyle = GUI.skin.FindStyle(Constants.InspectorPostHeaderStyleName) ?? GUIStyle.none;
            var inspectorHeaderStyle = GUI.skin.FindStyle(Constants.InspectorHeaderStyleName) ?? GUIStyle.none;
            var headerHeight = GetInspectorHeaderHeight(inspectorHeaderStyle);
            var headerY = GetInspectorHeaderY(postHeaderContentRect, postHeaderStyle, headerHeight);

            headerRect = new Rect(
                postHeaderContentRect.x - postHeaderStyle.padding.left,
                headerY,
                postHeaderContentRect.width + postHeaderStyle.padding.horizontal,
                headerHeight);
            return headerRect is { width: > 0f, height: > 0f };
        }

        private static float GetInspectorHeaderHeight(GUIStyle inspectorHeaderStyle)
        {
            var styleHeight = inspectorHeaderStyle.fixedHeight > 0f ? inspectorHeaderStyle.fixedHeight : 0f;
            var controlsHeight = Constants.InspectorHeaderTitleHeightFallback + EditorGUIUtility.singleLineHeight;
            var paddedControlsHeight = controlsHeight + inspectorHeaderStyle.padding.vertical;

            return Mathf.Max(
                Constants.InspectorHeaderImageSectionWidthFallback,
                paddedControlsHeight,
                styleHeight);
        }

        private static float GetInspectorHeaderY(
            Rect postHeaderContentRect,
            GUIStyle postHeaderStyle,
            float headerHeight)
        {
            var normalizedPostHeaderY = Mathf.Min(postHeaderContentRect.y, Constants.InspectorPostHeaderBaselineY);
            return normalizedPostHeaderY - postHeaderStyle.padding.top - headerHeight;
        }
    }
}
