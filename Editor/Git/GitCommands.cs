using UnityEditor;

using Executor = TalusKit.Editor.CommandLine.Executor;

namespace TalusKit.Editor.Git
{
    internal static class GitCommands
    {
        [MenuItem("TalusKit/Git/Update Submodules", false, 0)]
        private static void Run()
        {
            Executor.Execute("git submodule update --origin");
        }
    }
}