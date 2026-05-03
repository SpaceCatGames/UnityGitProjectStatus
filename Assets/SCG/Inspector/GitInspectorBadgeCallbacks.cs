using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SCG.GitProjectStatus
{
    /// <summary>
    /// Handles Inspector header badge drawing for persistent Unity assets.
    /// The callback resolves the primary inspected editor, maps it to a Git status entry, and draws one badge.
    /// </summary>
    internal static class GitInspectorBadgeCallbacks
    {
        private static readonly PropertyInfo s_firstInspectedEditorProperty = typeof(Editor)
            .GetProperty(Constants.FirstInspectedEditorPropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// Draws the Git badge after Unity finishes rendering the default Inspector header.
        /// The callback ignores import worker processes, unsupported editors, and repaint phases without a valid badge context.
        /// It is registered once during package bootstrap and reused for all Inspector headers.
        /// </summary>
        /// <param name="editor">Inspector editor whose default header has just been drawn.</param>
        internal static void OnFinishedDefaultHeaderGui(Editor editor)
        {
            var postHeaderContentRect = GUILayoutUtility.GetRect(0f, 0f, GUIStyle.none, GUILayout.ExpandWidth(true));

            if (AssetDatabase.IsAssetImportWorkerProcess() ||
                !GitProjectStatusSettings.InspectorBadgeEnabled ||
                !GitStatusCache.RepositoryDetected ||
                !TryCreateInspectorBadgeContext(editor, out var context) ||
                Event.current.type != EventType.Repaint)
                return;

            var entry = GitStatusCache.GetStatusForAssetPath(context.AssetPath);

            if (entry == null ||
                entry.Kind == GitStatusKind.None ||
                !TryGetInspectorHeaderRect(postHeaderContentRect, out var headerRect))
                return;

            var descriptor = GitStatusDescriptors.Get(entry.Kind);
            GitStatusBadgeGui.Draw(GetInspectorBadgeRect(headerRect), descriptor, GitProjectStatusSettings.CalcMode);
        }

        private static bool TryGetInspectorHeaderRect(Rect postHeaderContentRect, out Rect headerRect)
        {
            if (postHeaderContentRect.width <= 0f)
            {
                headerRect = default;
                return false;
            }

            var inspectorHeaderStyle = GUI.skin.FindStyle(Constants.InspectorHeaderStyleName) ?? GUIStyle.none;
            var postHeaderStyle = GUI.skin.FindStyle(Constants.InspectorPostHeaderStyleName) ?? GUIStyle.none;
            var headerHeight = GetInspectorHeaderHeight();
            var headerBottom = postHeaderContentRect.y - postHeaderStyle.padding.top +
                               Constants.InspectorHeaderBottomOverlap +
                               inspectorHeaderStyle.margin.bottom +
                               inspectorHeaderStyle.padding.bottom +
                               inspectorHeaderStyle.overflow.bottom;

            headerRect = new Rect(
                postHeaderContentRect.x - postHeaderStyle.padding.left,
                headerBottom - headerHeight,
                postHeaderContentRect.width + postHeaderStyle.padding.horizontal,
                headerHeight);
            return headerRect is { width: > 0f, height: > 0f };
        }

        private static Rect GetInspectorBadgeRect(Rect headerRect)
        {
            var iconSize = Mathf.Min(
                Constants.InspectorHeaderIconSize,
                Mathf.Max(0f, headerRect.height - Constants.InspectorHeaderContentInset * 2f));
            var iconRect = new Rect(
                headerRect.x + Constants.InspectorHeaderContentInset,
                headerRect.y + Constants.InspectorHeaderContentInset,
                iconSize,
                iconSize);
            var badgeSize = Mathf.Clamp(
                iconSize * Constants.InspectorHeaderBadgeSizeMultiplier,
                Constants.InspectorHeaderBadgeMinSize,
                Constants.InspectorHeaderBadgeMaxSize);

            return new Rect(
                iconRect.xMax - badgeSize + Constants.InspectorHeaderBadgeOffsetX,
                iconRect.y + Constants.InspectorHeaderBadgeOffsetY,
                badgeSize,
                badgeSize);
        }

        private static bool TryCreateInspectorBadgeContext(Editor editor, out InspectorBadgeContext context)
        {
            context = default;

            if (editor == null ||
                editor.target == null ||
                editor.targets.Length != 1 ||
                !IsInspectorBadgeOwner(editor) ||
                !TryResolveInspectorAssetPath(editor, out var assetPath, out var isImporterEditor))
                return false;

            context = new InspectorBadgeContext(assetPath, isImporterEditor);
            return true;
        }

        private static bool TryResolveInspectorAssetPath(Editor editor, out string assetPath, out bool isImporterEditor)
        {
            assetPath = string.Empty;
            isImporterEditor = false;

            if (editor.target is AssetImporter assetImporter)
            {
                assetPath = assetImporter.assetPath;
                isImporterEditor = true;
                return !string.IsNullOrEmpty(assetPath);
            }

            if (!EditorUtility.IsPersistent(editor.target))
            {
                return false;
            }

            assetPath = AssetDatabase.GetAssetPath(editor.target);
            return !string.IsNullOrEmpty(assetPath);
        }

        private static bool IsInspectorBadgeOwner(Editor editor) =>
            s_firstInspectedEditorProperty?.GetValue(editor, null) is bool firstInspectedEditor
                ? firstInspectedEditor
                : IsFallbackInspectorBadgeOwner(editor);

        private static bool IsFallbackInspectorBadgeOwner(Editor editor)
        {
            if (!TryResolveInspectorAssetPath(editor, out var assetPath, out var isImporterEditor))
                return false;

            var activeEditors = ActiveEditorTracker.sharedTracker.activeEditors;

            foreach (var activeEditor in activeEditors)
            {
                if (!TryResolveInspectorAssetPath(activeEditor, out var activeAssetPath, out var activeIsImporterEditor) ||
                    !string.Equals(activeAssetPath, assetPath, StringComparison.Ordinal))
                {
                    continue;
                }

                if (activeIsImporterEditor)
                {
                    return activeEditor == editor;
                }
            }

            foreach (var activeEditor in activeEditors)
            {
                if (!TryResolveInspectorAssetPath(activeEditor, out var activeAssetPath, out _) ||
                    !string.Equals(activeAssetPath, assetPath, StringComparison.Ordinal))
                {
                    continue;
                }

                return activeEditor == editor && !isImporterEditor;
            }

            return false;
        }

        private static float GetInspectorHeaderHeight() =>
            Mathf.Max(
                Constants.InspectorHeaderImageSectionWidthFallback,
                Constants.InspectorHeaderTitleHeightFallback + Constants.InspectorHeaderContentInset * 2f);
    }
}
