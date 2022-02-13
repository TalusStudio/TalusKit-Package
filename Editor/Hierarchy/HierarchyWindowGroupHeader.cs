using System;
using UnityEditor;
using UnityEngine;

namespace TalusKit.Editor.Hierarchy
{
    /// <summary>
    /// Hierarchy Window Group Header
    /// http://diegogiacomelli.com.br/unitytips-changing-the-style-of-the-hierarchy-window-group-header/
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyWindowGroupHeader
    {
        static readonly HierarchyWindowGroupHeaderSettings _Settings;
        static readonly GUIStyle _Style;

        static HierarchyWindowGroupHeader()
        {
            _Settings = HierarchyWindowGroupHeaderSettings.Instance;
            _Style = new GUIStyle();
            UpdateStyle();
            _Settings.Changed.AddListener(UpdateStyle);

            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        static void UpdateStyle()
        {
            _Style.fontSize = _Settings.FontSize;
            _Style.fontStyle = _Settings.FontStyle;
            _Style.alignment = _Settings.Alignment;
            _Style.normal.textColor = _Settings.TextColor;
            EditorApplication.RepaintHierarchyWindow();
        }

        static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (gameObject != null && gameObject.name.StartsWith(_Settings.NameStartsWith, StringComparison.Ordinal))
            {
                EditorGUI.DrawRect(selectionRect, _Settings.BackgroundColor);
                EditorGUI.LabelField(selectionRect, gameObject.name.Replace(_Settings.RemoveString, "").ToUpperInvariant(), _Style);
            }
        }
    }

}