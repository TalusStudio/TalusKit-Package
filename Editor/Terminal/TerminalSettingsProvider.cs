using UnityEditor;
using System;
using System.Linq;

namespace KRT.UnityTerminalLauncher
{
    class TerminalSettingsProvider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new TerminalSettingsProvider();
        }

        public TerminalSettingsProvider()
            : base("Preferences/Terminal Launcher", SettingsScope.User, new[] { "Terminal", "PowerShell", "ZSH" })
        {
        }

        public override bool HasSearchInterest(string searchContext)
        {
            return base.HasSearchInterest(searchContext);
        }

        public override void OnGUI(string searchContext)
        {
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
                    case TerminalType.PowerShellCore:
                        return "PowerShell Core";
                    case TerminalType.PowerShell:
                        return "PowerShell";
                    case TerminalType.Cmd:
                        return "Command Prompt";
                    case TerminalType.GitBash:
                        return "Git Bash";
                    case TerminalType.Zsh:
                        return "Zsh";
                    default:
                        throw new NotImplementedException($"Case for {t} is not implemented.");
                }
            })
            .ToArray();
    }
}