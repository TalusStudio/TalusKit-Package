using System.IO;
using System.Diagnostics;

using Microsoft.Win32;

namespace TalusKit.Editor.Terminal
{
    internal class GitBashLauncher : TerminalLauncher
    {
        internal override string LauncherName => "git-bash.exe";
        internal override bool HasExecutable => File.Exists(Path.Combine(GetGitInstallPath(), LauncherName));

        internal override void Launch(string targetFolder)
        {
            base.Launch(targetFolder);

            string gitInstallPath = GetGitInstallPath();
            string gitBash = Path.Combine(gitInstallPath, LauncherName);

            Process.Start(gitBash, $"--cd=\"{targetFolder}\"");
        }

        private static string GetGitInstallPath()
        {
            const string key = "HKEY_LOCAL_MACHINE\\SOFTWARE\\GitForWindows";
            return (string) Registry.GetValue(key, "InstallPath", "");
        }
    }
}