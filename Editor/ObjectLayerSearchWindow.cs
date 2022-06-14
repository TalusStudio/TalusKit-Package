using System.Collections.Generic;

using UnityEditorInternal;
using UnityEditor;
using UnityEngine;

namespace TalusKit.Editor
{
    internal class ObjectLayerSearchWindow : EditorWindow
    {
        private const string _IgnoreTag = "EditorOnly";

        private readonly List<GameObject> _LayerSearchResult = new List<GameObject>();
        private LayerMask _SearchedLayers;
        private Vector2 _SearchResultScroll = Vector2.zero;

        [MenuItem("TalusKit/Debugging/Object Layer Searcher")]
        public static void ShowWindow()
        {
            var window = GetWindow<ObjectLayerSearchWindow>();
            window.titleContent = new GUIContent("Object Layer Searcher");
            window.minSize = new Vector2(300, 300);
            window.Show();
        }

        private void OnGUI()
        {
            _SearchedLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(
                EditorGUILayout.MaskField(InternalEditorUtility.LayerMaskToConcatenatedLayersMask(_SearchedLayers),
                InternalEditorUtility.layers)
            );

            // data
            {
                _SearchResultScroll = EditorGUILayout.BeginScrollView(_SearchResultScroll);
                foreach (GameObject result in _LayerSearchResult)
                {
                    EditorGUILayout.ObjectField(result, typeof(GameObject), true);
                }
                EditorGUILayout.EndScrollView();
            }

            // button
            {
                GUILayout.FlexibleSpace();

                GUI.enabled = false;
                GUILayout.Label($"Ignored Tag: {_IgnoreTag}");
                GUI.enabled = true;

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Search", GUILayout.MinHeight(50)))
                {
                    _LayerSearchResult.Clear();

                    foreach (GameObject result in FindObjectsOfType<GameObject>(true))
                    {
                        if (_SearchedLayers != (_SearchedLayers | (1 << result.layer))) { continue; }
                        if (result.CompareTag(_IgnoreTag)) { continue; }

                        _LayerSearchResult.Add(result);
                    }
                }
            }
        }
    }
}