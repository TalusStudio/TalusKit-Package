using System.IO;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace TalusKit.Editor.iOS
{
    public class ExemptFromEncryption : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }

#if UNITY_IOS
            string plistPath = report.summary.outputPath + "/Info.plist";

            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            File.WriteAllText(plistPath, plist.WriteToString());
#endif
        }
    }
}