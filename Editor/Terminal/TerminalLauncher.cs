using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace TalusKit.Editor.Terminal
{
    internal abstract class TerminalLauncher
    {
        internal virtual bool HasExecutable => ExistsOnPath(LauncherName);
        internal abstract string LauncherName { get; }

        internal virtual void Launch(string targetFolder)
        {
            if (!HasExecutable)
            {
                throw new System.Exception("Terminal could not found!");
            }
        }

        protected bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        private static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }

            string values = Environment.GetEnvironmentVariable("PATH");
            return values.Split(Path.PathSeparator)
                .Select(path => Path.Combine(path, fileName))
                .FirstOrDefault(fullPath => File.Exists(fullPath));
        }
    }
}