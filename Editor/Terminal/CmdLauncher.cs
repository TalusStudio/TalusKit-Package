using System.Diagnostics;

namespace TalusKit.Editor.Terminal
{
    internal class CmdLauncher : TerminalLauncher
    {
        internal override string LauncherName => "cmd.exe";

        internal override void Launch(string targetFolder)
        {
            base.Launch(targetFolder);

            var processInfo = new ProcessStartInfo(LauncherName)
            {
                WorkingDirectory = targetFolder
            };

            Process.Start(processInfo);
        }
    }
}