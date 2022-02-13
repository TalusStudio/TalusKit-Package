using UnityEngine;
using UnityEngine.Events;

namespace TalusKit.Editor.Hierarchy
{
    [System.Serializable]
    public class HierarchyWindowGroupHeaderSettings
    {
        public UnityEvent onChanged = new UnityEvent();

        public string NameStartsWith = "---";
        public string RemoveString = "-";
        public FontStyle FontStyle = FontStyle.Bold;
        public int FontSize = 14;
        public TextAnchor Alignment = TextAnchor.MiddleRight;
        public Color TextColor = Color.white;
        public Color BackgroundColor = new Color(0f, 0.34f, 0.16f);
    }
}