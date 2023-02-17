using System.IO;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace TalusKit.Editor.CommandLine
{
    public static class Executor
    {
        public static void Execute(string command)
        {
            command = command.Replace("\"", "\"\"");
            string workingDir = Directory.GetCurrentDirectory();

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
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

            Debug.Log($"'{command}' running on: '{workingDir}'");

            proc.Start();

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            proc.WaitForExit();
        }
    }
}