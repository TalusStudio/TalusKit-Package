using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;

namespace TalusKit.Editor.BuildUtility
{
    public class IncrementBuildNumber : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }

            if (int.TryParse(PlayerSettings.iOS.buildNumber, out int currentBuildNumber))
            {
                string nextBuildNumber = (currentBuildNumber + 1).ToString();
                PlayerSettings.iOS.buildNumber = nextBuildNumber;

                Debug.Log("Setting new iOS build number to " + nextBuildNumber);
            }
            else
            {
                Debug.LogError("Failed to parse build number " + PlayerSettings.iOS.buildNumber + " as int.");
            }
        }
    }
}