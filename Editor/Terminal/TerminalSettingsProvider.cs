using UnityEditor;
using System;
using System.Linq;
using UnityEngine;

namespace TalusKit.Editor.Terminal
{
    internal class TerminalSettingsProvider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new TerminalSettingsProvider();
        }

        public TerminalSettingsProvider() : base("Talus Studio/Terminal Launcher", SettingsScope.User)
        { }

        public override bool HasSearchInterest(string searchContext)
        {
            return base.HasSearchInterest(searchContext);
        }

        public override void OnGUI(string searchContext)
        {
            GUILayout.Space(8);

            TerminalSettings.TerminalTypeInt = EditorGUILayout.Popup("Terminal", TerminalSettings.TerminalTypeInt, _TerminalTypeLabels);
        }

        private readonly string[] _TerminalTypeLabels = Enum.GetValues(typeof(TerminalType))
            .Cast<TerminalType>()
            .OrderBy(t => (int)t)
            .Select(t =>
            {
                switch (t)
                {
                    case TerminalType.Auto:
                        return "Auto";
                    case TerminalType.WindowsTerminal:
                        return "Windows Terminal";
                    case TerminalType.PowerShell:
                        return "PowerShell";
                    case TerminalType.Cmd:
                        return "Command Prompt";
                    case TerminalType.GitBash:
                        return "Git Bash";
                    case TerminalType.MacTerminal:
                        return "MacTerminal";
                    default:
                        throw new NotImplementedException($"Case for {t} is not implemented.");
                }
            })
            .ToArray();
    }
}