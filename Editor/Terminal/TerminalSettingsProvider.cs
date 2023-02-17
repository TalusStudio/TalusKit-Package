using System;
using System.Linq;

using UnityEditor;
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

        public override void OnGUI(string searchContext)
        {
            GUILayout.Space(8);

            TerminalSettings.TerminalTypeInt = EditorGUILayout.Popup("Terminal", TerminalSettings.TerminalTypeInt, _TerminalTypeLabels);
        }

        private readonly string[] _TerminalTypeLabels = Enum.GetValues(typeof(TerminalType))
            .Cast<TerminalType>()
            .OrderBy(terminal => (int)terminal)
            .Select(terminal =>
            {
                return terminal switch
                {
                    TerminalType.Auto => "Auto",
                    TerminalType.WindowsTerminal => "Windows Terminal",
                    TerminalType.PowerShell => "PowerShell",
                    TerminalType.Cmd => "Command Prompt",
                    TerminalType.GitBash => "Git Bash",
                    TerminalType.MacTerminal => "MacTerminal",
                    _ => throw new NotImplementedException($"Case for {terminal} is not implemented.")
                };
            })
            .ToArray();
    }
}