using System;
using UnityEditor;
using UnityEngine;

namespace SCG.UnityGitStatus
{
    /// <summary>
    /// Handles Project window row callbacks for badge and footer drawing.
    /// The callback resolves the visible asset path, finds the current Project Browser context, and renders overlays accordingly.
    /// It also delegates deleted-footer rendering to the dedicated footer helper.
    /// </summary>
    internal static class GitProjectOverlayCallbacks
    {
        /// <summary>
        /// Draws Project window overlays for one visible Project row.
        /// Rows are skipped when Git data is unavailable, the current callback does not map to a valid asset path, or settings disable drawing.
        /// The same callback also gives the deleted-files footer a chance to render.
        /// </summary>
        /// <param name="guid">Unity GUID associated with the visible Project row.</param>
        /// <param name="selectionRect">Rect describing the visible row being drawn.</param>
        internal static void OnProjectWindowItemGui(string guid, Rect selectionRect)
        {
            if (EditorApplication.isCompiling ||
                AssetDatabase.IsAssetImportWorkerProcess() ||
                !UnityGitStatusSettings.ProjectOverlaysEnabled ||
                !GitStatusCache.RepositoryDetected ||
                string.IsNullOrEmpty(guid))
                return;

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(assetPath) ||
                string.Equals(assetPath, Constants.AssetsRoot, StringComparison.Ordinal) ||
                string.Equals(assetPath, Constants.PackagesRoot, StringComparison.Ordinal))
                return;

            var hasProjectBrowserContext = ProjectBrowserContextResolver.TryGetContext(selectionRect, out var context);

            if (Event.current.type == EventType.Repaint)
            {
                var entry = GitStatusCache.GetStatusForAssetPath(assetPath);

                if (entry != null &&
                    entry.Kind != GitStatusKind.None &&
                    ShouldDrawProjectBadge(hasProjectBrowserContext, context))
                {
                    DrawProjectBadge(
                        selectionRect,
                        entry.Kind,
                        !hasProjectBrowserContext || !context.IsTwoColumns);
                }
            }

            GitProjectDeletedEntriesFooter.Draw(selectionRect, hasProjectBrowserContext, context);
        }

        private static bool ShouldDrawProjectBadge(bool hasProjectBrowserContext, ProjectBrowserContext context) =>
            !hasProjectBrowserContext ||
            !context.IsTwoColumns ||
            UnityGitStatusSettings.ShowLeftPaneOverlaysInTwoColumn ||
            !context.IsTreePane;

        private static void DrawProjectBadge(Rect itemRect, GitStatusKind kind, bool isOneColumnLayout)
        {
            var descriptor = GitStatusDescriptors.Get(kind);
            var badgeRect = UnityGitStatusSettings.RightAlignedBadges
                ? GetRightAlignedBadgeRect(itemRect, isOneColumnLayout)
                : GetIconCornerBadgeRect(itemRect);

            GitStatusBadgeGui.Draw(
                badgeRect,
                descriptor,
                UnityGitStatusSettings.CalcMode,
                GitStatusBadgeLayout.GetProjectBadgeContentOffset(UnityGitStatusSettings.RightAlignedBadges));
        }

        private static Rect GetRightAlignedBadgeRect(Rect itemRect, bool isOneColumnLayout)
        {
            var badgeSize = Mathf.Min(
                Constants.OverlayBadgeSize,
                Mathf.Max(Constants.OverlayBadgeMinSize, itemRect.height - 2f));
            var rightInset = isOneColumnLayout
                ? Constants.OverlayOneColumnRightAlignedBadgeRightInset
                : Constants.OverlayRightAlignedBadgeRightInset;

            return new Rect(
                itemRect.xMax - badgeSize - rightInset,
                itemRect.y + Constants.OverlayRightAlignedBadgeTopInset,
                badgeSize,
                badgeSize);
        }

        private static Rect GetIconCornerBadgeRect(Rect itemRect)
        {
            var iconSize = Mathf.Clamp(
                itemRect.height - 2f,
                Constants.OverlayIconCornerIconMinSize,
                Constants.OverlayIconCornerIconMaxSize);
            var iconRect = new Rect(
                itemRect.x + Constants.OverlayIconCornerIconLeftInset,
                itemRect.y + Mathf.Max(0f, (itemRect.height - iconSize) * 0.5f) + Constants.OverlayIconCornerIconTopInset,
                iconSize,
                iconSize);
            var badgeSize = Mathf.Clamp(
                iconSize * Constants.OverlayIconCornerBadgeSizeMultiplier,
                Constants.OverlayIconCornerBadgeMinSize,
                Constants.OverlayIconCornerBadgeMaxSize);

            return new Rect(
                iconRect.xMax - badgeSize + Constants.OverlayIconCornerBadgeOffsetX,
                iconRect.y + Constants.OverlayIconCornerBadgeOffsetY,
                badgeSize,
                badgeSize);
        }
    }
}
