using UnityEditor;

using UnityEngine;

namespace TalusKit.Editor
{
    internal class ReplaceWithPrefab : EditorWindow
    {
        [SerializeField] private GameObject _Prefab;
        [SerializeField] private bool _ApplyRotation = true;
        [SerializeField] private bool _ApplyScale = true;

        [MenuItem("TalusKit/Utility/Replace With Prefab %q", false, 200)]
        private static void CreateReplaceWithPrefab()
        {
            var window = GetWindow<ReplaceWithPrefab>();
            window.titleContent = new GUIContent("Replace with Prefab");
            window.minSize = new Vector2(370, 150);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(8);

            _Prefab = (GameObject) EditorGUILayout.ObjectField("Prefab", _Prefab, typeof(GameObject), false);
            _ApplyRotation = EditorGUILayout.Toggle("Apply Rotation", _ApplyRotation);
            _ApplyScale = EditorGUILayout.Toggle("Apply Scale", _ApplyScale);

            GUILayout.FlexibleSpace();

            GUI.enabled = false;
            EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
            GUI.enabled = true;

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Replace", GUILayout.MinHeight(50)))
            {
                GameObject[] selection = Selection.gameObjects;

                for (int i = selection.Length - 1; i >= 0; --i)
                {
                    GameObject selected = selection[i];
                    GameObject newObject;

                    if (PrefabUtility.IsPartOfPrefabAsset(_Prefab))
                    {
                        newObject = (GameObject) PrefabUtility.InstantiatePrefab(_Prefab);
                    }
                    else
                    {
                        newObject = Instantiate(_Prefab);
                        newObject.name = _Prefab.name;
                    }

                    if (newObject == null)
                    {
                        Debug.LogError("Error instantiating prefab!");
                        break;
                    }

                    Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
                    newObject.transform.parent = selected.transform.parent;
                    newObject.transform.localPosition = selected.transform.localPosition;

                    if (_ApplyRotation)
                    {
                        newObject.transform.localRotation = selected.transform.localRotation;
                    }

                    if (_ApplyScale)
                    {
                        newObject.transform.localScale = selected.transform.localScale;
                    }

                    newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                    Undo.DestroyObjectImmediate(selected);
                }
            }

            GUILayout.EndVertical();
        }
    }
}