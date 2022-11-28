using System.Diagnostics;

namespace TalusKit.Editor.Terminal
{
    internal class PowerShellLauncher : TerminalLauncher
    {
        internal override bool HasExecutable => ExistsOnPath("PowerShell.exe");

        internal override Process Launch(string targetFolder)
        {
            var processInfo = new ProcessStartInfo("PowerShell.exe")
            {
                WorkingDirectory = targetFolder
            };
            
            return Process.Start(processInfo);
        }
    }
}