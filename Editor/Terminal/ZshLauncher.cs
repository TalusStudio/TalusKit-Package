using System.Diagnostics;

namespace KRT.UnityTerminalLauncher
{
    class ZshLauncher : TerminalLauncher
    {
        internal override bool HasExecutable => ExistsOnPath("/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal");

        internal override Process Launch(string targetFolder)
        {
            UnityEngine.Debug.Log(targetFolder);
            string command = @"open -a Terminal " + targetFolder;
            command = command.Replace(@"\", "/");

            UnityEngine.Debug.Log(command);

            var startInfo = new ProcessStartInfo()
            {
                FileName = "bash",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = " -c \"" + command + " \""
            };

            return Process.Start(startInfo);
        }
    }
}