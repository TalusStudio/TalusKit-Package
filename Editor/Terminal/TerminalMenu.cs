using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace TalusKit.Editor.Terminal
{
    internal static class TerminalMenu
    {
        [MenuItem("Assets/Open Terminal Here %t")]
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
            MethodInfo activeFolderPath = typeof(ProjectWindowUtil).GetMethod(
                "TryGetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            object[] args = { null };
            bool found = (bool) activeFolderPath.Invoke(null, args);
            string path = (string) args[0];
            path = path.Trim('/');

            return found ? path : GetProjectPath();
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