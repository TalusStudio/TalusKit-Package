using System.Collections.Generic;

using UnityEngine;

namespace TalusKit.Editor.CheckWindow
{
    [AddComponentMenu("Ignored Check Results (Do not use directly)", 0)]
    public class IgnoredCheckResults : MonoBehaviour
    {
        [System.Serializable]
        public struct IgnoredCheckResult
        {
            public string Check;
            public GameObject GameObject;
        }

        public List<IgnoredCheckResult> IgnoredResults;
    }
}
