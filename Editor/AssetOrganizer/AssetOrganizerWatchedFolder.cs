using UnityEditor;
using UnityEngine;

using Sirenix.OdinInspector;

namespace TalusKit.Editor.AssetOrganizer
{
    [CreateAssetMenu(menuName = "_OTHERS/Asset Organizer/Watched Folder")]
    internal class AssetOrganizerWatchedFolder : ScriptableObject
    {
        [FolderPath(UseBackslashes = true)]
        [ValueColor(nameof(PathStatusColor))]
        [OnValueChanged(nameof(DisableSubdirectoriesIfNecessary))]
        public string Path;

        [DisableIf("@Path == \"Assets\"")]
        [TableColumnWidth(150, Resizable = false)]
        [ValueDropdown(nameof(subdirectoryOptions))]
        public bool IncludeSubdirectories;

        private readonly ValueDropdownList<bool> subdirectoryOptions = new ValueDropdownList<bool>
        {
            { "Yes", true },
            { "No", false }
        };

        private Color PathStatusColor => AssetDatabase.IsValidFolder(Path)
            ? AssetOrganizerColors.Success
            : AssetOrganizerColors.Error;

        private void DisableSubdirectoriesIfNecessary()
        {
            if (Path == "Assets")
            {
                IncludeSubdirectories = false;
            }
        }
    }
}
