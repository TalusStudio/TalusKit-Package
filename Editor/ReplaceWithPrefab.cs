using UnityEditor;

using UnityEngine;

namespace TalusKit.Editor
{
    public class ReplaceWithPrefab : EditorWindow
    {
        [SerializeField]
        private GameObject _Prefab;

        [SerializeField]
        private bool _ReplaceRotation = true;

        [SerializeField]
        private bool _ReplaceScale = true;

        private void OnGUI()
        {
            _Prefab = (GameObject) EditorGUILayout.ObjectField("Prefab", _Prefab, typeof(GameObject), false);
            _ReplaceRotation = EditorGUILayout.Toggle("Replace Rotation", _ReplaceRotation);
            _ReplaceScale = EditorGUILayout.Toggle("Replace Scale", _ReplaceScale);

            if (GUILayout.Button("Replace"))
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

                    if (_ReplaceRotation)
                    {
                        newObject.transform.localRotation = selected.transform.localRotation;
                    }

                    if (_ReplaceScale)
                    {
                        newObject.transform.localScale = selected.transform.localScale;
                    }

                    newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                    Undo.DestroyObjectImmediate(selected);
                }
            }

            GUI.enabled = false;
            EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
        }

        [MenuItem("TalusKit/Replace With Prefab %q", false, -999)]
        private static void CreateReplaceWithPrefab()
        {
            GetWindow<ReplaceWithPrefab>().titleContent = new GUIContent("Replace with Prefab");
            GetWindow<ReplaceWithPrefab>().Show();
        }
    }
}