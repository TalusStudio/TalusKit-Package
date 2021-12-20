using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace TalusKit.Editor.CheckWindow.Checks
{
    public class InvalidTransformCheck : Check
    {
        public override string Name => "GameObject/Invalid Transform";
        public override bool DefaultEnabled => true;
        public override string[] ResolutionActions => new string[] { "Do Nothing", "Reset", "Delete Object" };

        public override IEnumerable<CheckResult> GetResults(SceneObjects so)
        {
            try
            {
                int count = so.AllObjects.Length;
                int i = 0;

                foreach (GameObject go in so.AllObjects)
                {
                    float progress = ++i / count;

                    if (EditorUtility.DisplayCancelableProgressBar("Finding Transforms...", $"{go.name}", progress))
                    {
                        break;
                    }

                    Transform t = go.transform;

                    if (t.localScale.sqrMagnitude == 0)
                    {
                        yield return new CheckResult(this, CheckResult.Result.Warning,
                            $"Transform of Game Object \"{go.name}\" has zero Scale.", go);
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
            throw new System.NotImplementedException();
        }
    }
}
