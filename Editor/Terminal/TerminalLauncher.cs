using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

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