using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Helpers
{
    public static class EditorManagerHelper
    {
        public static Dictionary<FolderEntryViewModel, EditorControl> Editors { get; set; } = [];

        public static void AutoSaveEditors()
        {
            var editorsToRemove = new List<FolderEntryViewModel>();

            foreach (var kvp in Editors)
            {
                var editor = kvp.Value;
                if (editor.IsDirty)
                {
                    editor.AutoSave();
                }
                else if ((DateTime.Now - editor.LastSaveTime).TotalMinutes > 10
                    && !kvp.Value.IsActive)
                {
                    editorsToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in editorsToRemove)
            {
                Editors[key].Dispose();
                Editors.Remove(key);
            }
        }
    }
}
