using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace TalusKit.Editor.CheckWindow.Checks
{
    public class EmptyGameObjectCheck : Check
    {
        public override string Name => "GameObject/Empty";
        public override bool DefaultEnabled => true;
        public override string[] ResolutionActions => new string[] { "Set Static", "Delete Object" };

        public override IEnumerable<CheckResult> GetResults(SceneObjects so)
        {
            try
            {
                int count = so.AllObjects.Length;
                int i = 0;

                foreach (GameObject go in so.AllObjects)
                {
                    float progress = ++i / count;

                    if (EditorUtility.DisplayCancelableProgressBar("Finding Empty Game Objects...", $"{go.name}", progress))
                    {
                        break;
                    }

                    Component[] allComps = go.GetComponents<Component>();

                    if (allComps.Length != 1)
                    {
                        continue;
                    }

                    if (!go.isStatic)
                    {
                        if (!so.ReferencedGameObjects.Contains(go))
                        {
                            var result = new CheckResult(this, CheckResult.Result.Warning,
                                $"Empty Game Object {go.name} is not static", go);

                            result.ResolutionActionIndex = 0;
                            yield return result;
                        }
                    }
                    else
                    {
                        if (go.transform.childCount == 0)
                        {
                            if (!so.ReferencedGameObjects.Contains(go) && !so.ReferencedComponents.Contains(go.transform))
                            {
                                var result = new CheckResult(this, CheckResult.Result.Notice,
                                    "Empty Static Game Object is not referenced, and has no children", go);

                                result.ResolutionActionIndex = 1;
                                yield return result;
                            }
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public override void Resolve(CheckResult result)
        {
            switch (result.ResolutionActionIndex)
            {
                default:
                    (result.MainObject as GameObject).isStatic = true;
                    break;

                case 1:
                    Object.DestroyImmediate(result.MainObject as GameObject);
                    break;
            }
        }
    }
}
