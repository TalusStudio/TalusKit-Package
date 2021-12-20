using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace TalusKit.Editor.CheckWindow.Checks
{
    public class NullMaterialCheck : Check
    {
        static Material _DefaultMaterial;

        public override string Name => "Renderer/Null Material";
        public override bool DefaultEnabled => true;
        public override string[] ResolutionActions => new[] { "Assign Default Material" };

        public override IEnumerable<CheckResult> GetResults(SceneObjects sceneObjects)
        {
            foreach (GameObject obj in sceneObjects.AllObjects)
            {
                if (!obj.TryGetComponent(out Renderer renderer))
                {
                    continue;
                }

                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (mat == null)
                    {
                        yield return new CheckResult(this, CheckResult.Result.Failed,
                            $"Missing Material for Object {obj.name}", obj);

                        break;
                    }
                }
            }
        }

        public override void Resolve(CheckResult result)
        {
            if (_DefaultMaterial == null)
            {
                _DefaultMaterial = (Material)AssetDatabase.LoadAssetAtPath("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat", typeof(Material));
                Debug.Log(_DefaultMaterial != null ? "Default Material found!" : "Default Material not found!");
            }

            switch (result.ResolutionActionIndex)
            {
                default:
                {
                    Renderer renderer = (result.MainObject as GameObject).GetComponent<Renderer>();

                    var serializedData = new SerializedObject(renderer);
                    serializedData.Update();

                    SerializedProperty serializedProperty = serializedData.FindProperty("m_Materials");
                    int materialCount = serializedProperty.arraySize;

                    for (int i = 0; i < materialCount; i++)
                    {
                        SerializedProperty property = serializedProperty.GetArrayElementAtIndex(i);

                        if (property.objectReferenceValue == null)
                        {
                            property.objectReferenceValue = _DefaultMaterial;
                        }
                    }

                    if (serializedData.hasModifiedProperties)
                    {
                        serializedData.ApplyModifiedProperties();
                    }
                } break;
            }
        }
    }
}
