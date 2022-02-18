using System.Diagnostics;

namespace KRT.UnityTerminalLauncher
{
    class ZshLauncher : TerminalLauncher
    {
        internal override bool HasExecutable => ExistsOnPath("/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal");

        internal override Process Launch(string targetFolder)
        {
            var processInfo = new ProcessStartInfo("/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal")
            {
                WorkingDirectory = targetFolder,
            };

            return Process.Start(processInfo);
        }
    }
}