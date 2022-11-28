using System.IO;
using System.Diagnostics;

using Microsoft.Win32;

namespace TalusKit.Editor.Terminal
{
    internal class GitBashLauncher : TerminalLauncher
    {
        internal override bool HasExecutable => File.Exists(Path.Combine(GetGitInstallPath(), "git-bash.exe"));

        internal override Process Launch(string targetFolder)
        {
            string gitInstallPath = GetGitInstallPath();
            string gitBash = Path.Combine(gitInstallPath, "git-bash.exe");
            return Process.Start(gitBash, $"--cd=\"{targetFolder}\"");
        }

        private static string GetGitInstallPath()
        {
            const string key = "HKEY_LOCAL_MACHINE\\SOFTWARE\\GitForWindows";
            return (string) Registry.GetValue(key, "InstallPath", "");
        }
    }
}