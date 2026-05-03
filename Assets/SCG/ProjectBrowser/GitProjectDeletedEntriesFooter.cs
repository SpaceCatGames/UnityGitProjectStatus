using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Draws the deleted-files footer inside the Project window when space allows it.
    /// The footer is limited to Project Browser contexts where deleted children belong to the active folder.
    /// It stays hidden when the visible Project content could overlap the footer area.
    /// </summary>
    internal static class GitProjectDeletedEntriesFooter
    {
        private static GUIStyle s_deletedFooterItemStyle;
        private static GUIStyle s_deletedFooterBackgroundStyle;
        private static readonly Dictionary<int, int> s_footerMeasureFrameByProjectBrowser = new();
        private static readonly Dictionary<int, float> s_currentFooterMeasureMaxRowBottomByProjectBrowser = new();
        private static readonly Dictionary<int, float> s_previousFooterMeasureMaxRowBottomByProjectBrowser = new();
        private static int s_lastFooterFrame;
        private static EventType s_lastFooterEventType;
        private static int s_lastFooterProjectBrowserInstanceId;

        /// <summary>
        /// Draws the deleted-files footer for the current Project window callback when the active context qualifies.
        /// The method resolves the active folder, measures visible rows, and suppresses the footer when content overlap is possible.
        /// One-column layout and non-right-pane contexts are ignored intentionally.
        /// </summary>
        /// <param name="selectionRect">Current Project row rect provided by Unity.</param>
        /// <param name="hasProjectBrowserContext">Whether a valid Project Browser context was resolved for the row.</param>
        /// <param name="context">Resolved Project Browser geometry and pane metadata.</param>
        internal static void Draw(Rect selectionRect, bool hasProjectBrowserContext, ProjectBrowserContext context)
        {
            if (!GitProjectStatusSettings.ShowDeletedFilesInProject ||
                !IsRelevantProjectEvent(Event.current.type) ||
                !hasProjectBrowserContext ||
                !context.IsTwoColumns ||
                !context.IsRightPane)
                return;

            if (!IsSelectionInRightPane(context, selectionRect))
                return;

            TrackFooterRepaintMeasurement(context, GetRowBottomInVisibleRectSpace(context, selectionRect));

            if (!ShouldDrawFooterForCurrentEvent(context))
                return;

            var contextFolderPath = ProjectBrowserContextResolver.GetActiveFolderPath(context);

            if (string.IsNullOrEmpty(contextFolderPath))
                return;

            var deletedEntries = FilterProjectDeletedEntries(GitStatusCache.GetDeletedEntriesForFolder(contextFolderPath));

            if (deletedEntries.Count == 0)
            {
                return;
            }

            var isExpanded = GitProjectStatusSettings.ShowProjectDeletedEntries;
            var availableHeight = GetAvailableFooterHeight(context);
            CalculateFooterRows(
                deletedEntries.Count,
                isExpanded,
                availableHeight,
                out var visibleItemCount,
                out var showSummaryRow);
            var footerRect = GetDeletedFooterRect(
                context.VisibleRect,
                visibleItemCount,
                showSummaryRow);

            if (!HasEnoughFooterSpace(GetDeletedFooterTopInVisibleRectSpace(context, visibleItemCount, showSummaryRow), context))
            {
                return;
            }

            var headerRect = new Rect(
                footerRect.x + Constants.ProjectDeletedFooterPadding,
                footerRect.y + Constants.ProjectDeletedFooterPadding,
                footerRect.width - Constants.ProjectDeletedFooterPadding * 2f,
                EditorGUIUtility.singleLineHeight);

            if (Event.current.type == EventType.Repaint)
            {
                GetDeletedFooterBackgroundStyle().Draw(footerRect, GUIContent.none, false, false, false, false);
            }

            var nextExpanded = EditorGUI.Foldout(
                headerRect,
                GitProjectStatusSettings.ShowProjectDeletedEntries,
                GetDeletedFooterHeaderText(contextFolderPath, deletedEntries.Count),
                true);

            if (nextExpanded != GitProjectStatusSettings.ShowProjectDeletedEntries)
            {
                GitProjectStatusSettings.ShowProjectDeletedEntries = nextExpanded;
            }

            if (!GitProjectStatusSettings.ShowProjectDeletedEntries)
            {
                return;
            }

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var y = headerRect.yMax + Constants.ProjectDeletedFooterRowSpacing;
            var itemStyle = GetDeletedFooterItemStyle();

            for (var index = 0; index < visibleItemCount; index++)
            {
                var itemRect = new Rect(headerRect.x + 14f, y, headerRect.width - 14f, lineHeight);
                EditorGUI.LabelField(itemRect, GetDeletedEntryLabel(contextFolderPath, deletedEntries[index]), itemStyle);
                y += lineHeight + Constants.ProjectDeletedFooterRowSpacing;
            }

            if (!showSummaryRow)
            {
                return;
            }

            var remainingRect = new Rect(headerRect.x + 14f, y, headerRect.width - 14f, lineHeight);
            EditorGUI.LabelField(
                remainingRect,
                $"+{deletedEntries.Count - visibleItemCount} more deleted file(s)",
                itemStyle);
        }

        private static Rect GetDeletedFooterRect(Rect visibleRect, int visibleItemCount, bool showSummaryRow)
        {
            var height = GetDeletedFooterHeight(visibleItemCount, showSummaryRow);
            return new Rect(
                visibleRect.x + Constants.ProjectDeletedFooterPadding,
                visibleRect.yMax - height - Constants.ProjectDeletedFooterPadding,
                visibleRect.width - Constants.ProjectDeletedFooterPadding * 2f,
                height);
        }

        private static string GetDeletedFooterHeaderText(string activeFolderPath, int deletedEntryCount)
        {
            var folderName = GetFileName(activeFolderPath);
            var actionText = GitProjectStatusSettings.ShowProjectDeletedEntries ? "Hide" : "Show";
            return $"{actionText} deleted files in {folderName} ({deletedEntryCount})";
        }

        private static List<GitStatusEntry> FilterProjectDeletedEntries(IReadOnlyList<GitStatusEntry> deletedEntries)
        {
            var filteredEntries = new List<GitStatusEntry>(deletedEntries.Count);
            filteredEntries.AddRange(deletedEntries.Where(deletedEntry => !deletedEntry.IsMeta));
            return filteredEntries;
        }

        private static float GetAvailableFooterHeight(ProjectBrowserContext context)
        {
            var previousMaxRowBottom = GetPreviousFooterMeasureMaxRowBottom(context);

            return float.IsNegativeInfinity(previousMaxRowBottom)
                ? float.PositiveInfinity
                : Mathf.Max(
                    0f,
                    context.VisibleRect.height - previousMaxRowBottom - Constants.ProjectDeletedFooterPadding);
        }

        private static void CalculateFooterRows(
            int deletedEntryCount,
            bool isExpanded,
            float availableHeight,
            out int visibleItemCount,
            out bool showSummaryRow)
        {
            visibleItemCount = 0;
            showSummaryRow = false;

            if (!isExpanded || deletedEntryCount <= 0)
                return;

            if (float.IsPositiveInfinity(availableHeight))
            {
                visibleItemCount = Mathf.Min(deletedEntryCount, Constants.ProjectDeletedFooterMaxVisibleItems);
                showSummaryRow = deletedEntryCount > visibleItemCount;
                return;
            }

            var collapsedHeight = GetDeletedFooterHeight(0, false);
            var rowHeight = EditorGUIUtility.singleLineHeight + Constants.ProjectDeletedFooterRowSpacing;
            var rowsCapacity = Mathf.Max(0, Mathf.FloorToInt((availableHeight - collapsedHeight) / rowHeight));

            if (rowsCapacity <= 0)
            {
                return;
            }

            var preferredVisibleItemCount = Mathf.Min(deletedEntryCount, Constants.ProjectDeletedFooterMaxVisibleItems);

            if (deletedEntryCount <= preferredVisibleItemCount)
            {
                visibleItemCount = Mathf.Min(rowsCapacity, deletedEntryCount);
                return;
            }

            if (rowsCapacity == 1)
            {
                showSummaryRow = true;
                return;
            }

            visibleItemCount = Mathf.Min(rowsCapacity - 1, preferredVisibleItemCount);
            showSummaryRow = deletedEntryCount > visibleItemCount;
        }

        private static float GetDeletedFooterHeight(int visibleItemCount, bool showSummaryRow)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var contentLineCount = 1 + visibleItemCount + (showSummaryRow ? 1 : 0);
            return
                Constants.ProjectDeletedFooterPadding * 2f +
                contentLineCount * lineHeight +
                contentLineCount * Constants.ProjectDeletedFooterRowSpacing;
        }

        private static string GetDeletedEntryLabel(string activeFolderPath, GitStatusEntry entry)
        {
            var displayPath = GitStatusEntry.NormalizePath(entry.DisplayPath);
            var normalizedFolderPath = GitStatusEntry.NormalizePath(activeFolderPath);

            return displayPath.StartsWith(normalizedFolderPath + "/", StringComparison.Ordinal)
                ? displayPath[(normalizedFolderPath.Length + 1)..]
                : GetFileName(displayPath);
        }

        private static bool IsRelevantProjectEvent(EventType eventType) =>
            eventType is EventType.Layout or EventType.Repaint or EventType.MouseDown or EventType.MouseUp;

        private static void TrackFooterRepaintMeasurement(ProjectBrowserContext context, float measuredRowBottom)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var instanceId = context.InstanceId;

            if (!s_footerMeasureFrameByProjectBrowser.TryGetValue(instanceId, out var trackedFrame) ||
                trackedFrame != Time.frameCount)
            {
                s_previousFooterMeasureMaxRowBottomByProjectBrowser[instanceId] =
                    s_currentFooterMeasureMaxRowBottomByProjectBrowser.GetValueOrDefault(instanceId, float.NegativeInfinity);
                s_footerMeasureFrameByProjectBrowser[instanceId] = Time.frameCount;
                s_currentFooterMeasureMaxRowBottomByProjectBrowser[instanceId] = measuredRowBottom;
                return;
            }

            s_currentFooterMeasureMaxRowBottomByProjectBrowser[instanceId] =
                s_currentFooterMeasureMaxRowBottomByProjectBrowser.TryGetValue(instanceId, out var maxRowBottom)
                    ? Mathf.Max(maxRowBottom, measuredRowBottom)
                    : measuredRowBottom;
        }

        private static bool HasEnoughFooterSpace(float footerTop, ProjectBrowserContext context)
        {
            var previousMaxRowBottom = GetPreviousFooterMeasureMaxRowBottom(context);
            return float.IsNegativeInfinity(previousMaxRowBottom) || previousMaxRowBottom <= footerTop;
        }

        private static float GetPreviousFooterMeasureMaxRowBottom(ProjectBrowserContext context) =>
            s_previousFooterMeasureMaxRowBottomByProjectBrowser.GetValueOrDefault(context.InstanceId, float.NegativeInfinity);

        private static bool IsSelectionInRightPane(ProjectBrowserContext context, Rect selectionRect)
        {
            var translatedSelectionRect = new Rect(
                selectionRect.x + context.VisibleRect.x,
                selectionRect.y + context.VisibleRect.y,
                selectionRect.width,
                selectionRect.height);
            var rawListScore = GetOverlapScore(selectionRect, context.ListAreaRect);
            var rawTreeScore = GetOverlapScore(selectionRect, context.TreeViewRect);
            var translatedListScore = GetOverlapScore(translatedSelectionRect, context.ListAreaRect);
            var translatedTreeScore = GetOverlapScore(translatedSelectionRect, context.TreeViewRect);
            var rawBestScore = Mathf.Max(rawListScore, rawTreeScore);
            var translatedBestScore = Mathf.Max(translatedListScore, translatedTreeScore);
            var useTranslatedSelectionRect = translatedBestScore > rawBestScore;
            var listScore = useTranslatedSelectionRect ? translatedListScore : rawListScore;
            var treeScore = useTranslatedSelectionRect ? translatedTreeScore : rawTreeScore;

            return listScore > 0f && listScore > treeScore;
        }

        private static float GetRowBottomInVisibleRectSpace(ProjectBrowserContext context, Rect selectionRect)
        {
            const float tolerance = 6f;
            var rawBottom = selectionRect.yMax;
            var offsetBottom = selectionRect.yMax - context.VisibleRect.yMin;
            var visibleHeight = context.VisibleRect.height;
            var rawFits = rawBottom >= -tolerance && rawBottom <= visibleHeight + tolerance;
            var offsetFits = offsetBottom >= -tolerance && offsetBottom <= visibleHeight + tolerance;

            if (rawFits && !offsetFits)
            {
                return Mathf.Clamp(rawBottom, 0f, visibleHeight);
            }

            if (offsetFits && !rawFits)
            {
                return Mathf.Clamp(offsetBottom, 0f, visibleHeight);
            }

            var rawDistance = GetDistanceToRange(rawBottom, 0f, visibleHeight);
            var offsetDistance = GetDistanceToRange(offsetBottom, 0f, visibleHeight);
            var localBottom = offsetDistance < rawDistance ? offsetBottom : rawBottom;
            return Mathf.Clamp(localBottom, 0f, visibleHeight);
        }

        private static float GetDeletedFooterTopInVisibleRectSpace(
            ProjectBrowserContext context,
            int visibleItemCount,
            bool showSummaryRow)
        {
            var footerHeight = GetDeletedFooterHeight(visibleItemCount, showSummaryRow);
            return Mathf.Max(0f, context.VisibleRect.height - footerHeight - Constants.ProjectDeletedFooterPadding);
        }

        private static float GetDistanceToRange(float value, float min, float max) =>
            value < min
                ? min - value
                : value > max
                    ? value - max
                    : 0f;

        private static float GetOverlapScore(Rect source, Rect target)
        {
            var xMin = Mathf.Max(source.xMin, target.xMin);
            var yMin = Mathf.Max(source.yMin, target.yMin);
            var xMax = Mathf.Min(source.xMax, target.xMax);
            var yMax = Mathf.Min(source.yMax, target.yMax);

            return xMax <= xMin || yMax <= yMin ? 0f : (xMax - xMin) * (yMax - yMin);
        }

        private static bool ShouldDrawFooterForCurrentEvent(ProjectBrowserContext context)
        {
            if (s_lastFooterFrame == Time.frameCount &&
                s_lastFooterEventType == Event.current.type &&
                s_lastFooterProjectBrowserInstanceId == context.InstanceId)
                return false;

            s_lastFooterFrame = Time.frameCount;
            s_lastFooterEventType = Event.current.type;
            s_lastFooterProjectBrowserInstanceId = context.InstanceId;
            return true;
        }

        private static string GetFileName(string path)
        {
            var normalizedPath = GitStatusEntry.NormalizePath(path);
            var slashIndex = normalizedPath.LastIndexOf('/');
            return slashIndex >= 0 ? normalizedPath[(slashIndex + 1)..] : normalizedPath;
        }

        private static GUIStyle GetDeletedFooterItemStyle()
        {
            if (s_deletedFooterItemStyle != null)
            {
                return s_deletedFooterItemStyle;
            }

            s_deletedFooterItemStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal =
                {
                    textColor = new Color(0.95f, 0.18f, 0.18f, 1f)
                }
            };

            return s_deletedFooterItemStyle;
        }

        private static GUIStyle GetDeletedFooterBackgroundStyle()
        {
            s_deletedFooterBackgroundStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };

            return s_deletedFooterBackgroundStyle;
        }
    }
}
