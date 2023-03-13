using System.IO;
using System.Diagnostics;

using TalusKit.Editor.Terminal;

using Debug = UnityEngine.Debug;

namespace TalusKit.Editor.CommandLine
{
    public static class Executor
    {
        public static void Execute(string command)
        {
            command = command.Replace("\"", "\"\"");
            string workingDir = Directory.GetCurrentDirectory();

            string terminal = TerminalSettings.TerminalType == TerminalType.MacTerminal
                ? "bash"
                : "cmd";

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = terminal,
                    Arguments = "/c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDir
                }
            };

            proc.OutputDataReceived += (sender, e) => Debug.Log(e.Data);
            proc.ErrorDataReceived += (sender, e) => Debug.LogError(e.Data);

            Debug.Log($"'{command}' running in {terminal} shell. Working Path: '{workingDir}'");

            proc.Start();

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            proc.WaitForExit();
        }
    }
}