using System.Diagnostics;

namespace TalusKit.Editor.Terminal
{
    internal class MacTerminalLauncher : TerminalLauncher
    {
        internal override string LauncherName => "/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";

        internal override void Launch(string targetFolder)
        {
            base.Launch(targetFolder);

            string command = @"open -a Terminal " + targetFolder;
            command = command.Replace(@"\", "/");

            var startInfo = new ProcessStartInfo
            {
                FileName = "bash",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = " -c \"" + command + " \""
            };

            Process.Start(startInfo);
        }
    }
}