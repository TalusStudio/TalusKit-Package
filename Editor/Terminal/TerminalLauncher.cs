using System;
using System.Diagnostics;
using System.IO;

namespace TalusKit.Editor.Terminal
{
    internal abstract class TerminalLauncher
    {
        internal abstract bool HasExecutable { get; }
        internal abstract Process Launch(string targetFolder);

        protected bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        private static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            string values = Environment.GetEnvironmentVariable("PATH");
            foreach (string path in values.Split(Path.PathSeparator))
            {
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }
    }
}