using UnityEditor;

using UnityEngine;
using UnityEngine.Events;

namespace TalusKit.Editor.Hierarchy
{
    public class HierarchyWindowGroupHeaderSettings : ScriptableObject
    {
        [HideInInspector]
        public UnityEvent Changed;

        public string NameStartsWith = "---";
        public string RemoveString = "-";
        public FontStyle FontStyle = FontStyle.Bold;
        public int FontSize = 14;
        public TextAnchor Alignment = TextAnchor.MiddleCenter;
        public Color TextColor = Color.black;
        public Color BackgroundColor = Color.gray;

        static HierarchyWindowGroupHeaderSettings _Instance;
        public static HierarchyWindowGroupHeaderSettings Instance => _Instance != null ? _Instance : _Instance = LoadAsset();

        private void OnValidate()
        {
            Changed?.Invoke();
        }

        private static HierarchyWindowGroupHeaderSettings LoadAsset()
        {
            string path = GetAssetDir() + "/Resources/Settings.asset";
            var asset = (HierarchyWindowGroupHeaderSettings) AssetDatabase.LoadAssetAtPath(path, typeof(HierarchyWindowGroupHeaderSettings));
            return asset;
        }

        private static string GetAssetDir()
        {
            string pathOfFile = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("HierarchyWindowGroupHeader")[0]);
            return pathOfFile.Substring(0, pathOfFile.IndexOf("HierarchyWindowGroupHeader.cs"));
        }

    }
}