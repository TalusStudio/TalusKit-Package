using UnityEngine;
using UnityEditor;
using System.IO;

namespace TalusKit.Editor.Terminal
{
    static class TerminalMenu
    {
        [MenuItem("Assets/Open Terminal Here")]
        private static void OpenTerminalHere()
        {
            string path = GetSelectedPathOrFallback();
            string cd = Path.Combine(GetProjectPath(), path).Replace('/', '\\');
            TerminalLauncher launcher = CreateLauncher(TerminalSettings.TerminalType);
            launcher.Launch(cd);
        }

        private static string GetProjectPath()
        {
            string assetsPath = Application.dataPath;
            return assetsPath.Substring(0, assetsPath.Length - "Assets".Length).Replace(":/", "://");
        }

        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    continue;
                }

                path = Path.GetDirectoryName(path);
                break;
            }
            return path;
        }

        private static TerminalLauncher CreateLauncher(TerminalType terminalType)
        {
            switch (terminalType)
            {
                case TerminalType.Auto:
                    foreach (TerminalType t in System.Enum.GetValues(typeof(TerminalType)))
                    {
                        if (t == TerminalType.Auto) { continue; }
                        TerminalLauncher launcher = CreateLauncher(t);
                        if (launcher.HasExecutable) { return launcher; }
                    }
                    throw new System.Exception("Suitable terminal not found in system.");
                case TerminalType.WindowsTerminal:
                    return new WindowsTerminalLauncher();
                case TerminalType.PowerShellCore:
                    return new PowerShellCoreLauncher();
                case TerminalType.PowerShell:
                    return new PowerShellLauncher();
                case TerminalType.Cmd:
                    return new CmdLauncher();
                case TerminalType.GitBash:
                    return new GitBashLauncher();
                case TerminalType.MacTerminal:
                    return new MacTerminalLauncher();
                default:
                    throw new System.NotImplementedException($"Launcher for {terminalType} is not implemented.");
            }
        }
    }
}