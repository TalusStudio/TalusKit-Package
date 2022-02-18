using System.Diagnostics;

namespace KRT.UnityTerminalLauncher
{
    class ZshLauncher : TerminalLauncher
    {
        internal override bool HasExecutable => ExistsOnPath("/bin/zsh");

        internal override Process Launch(string targetFolder)
        {
            var processInfo = new ProcessStartInfo("/bin/zsh")
            {
                WorkingDirectory = targetFolder,
            };

            return Process.Start(processInfo);
        }
    }
}