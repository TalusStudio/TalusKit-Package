using System.Diagnostics;

namespace TalusKit.Editor.Terminal
{
    internal class CmdLauncher : TerminalLauncher
    {
        internal override bool HasExecutable => ExistsOnPath("cmd.exe");

        internal override Process Launch(string targetFolder)
        {
            var processInfo = new ProcessStartInfo("cmd.exe")
            {
                WorkingDirectory = targetFolder,
            };
            return Process.Start(processInfo);
        }
    }
}