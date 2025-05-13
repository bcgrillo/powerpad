using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Collections.Generic;

namespace PowerPad.WinUI.Helpers
{
    /// <summary>
    /// Provides helper methods for managing editor instances, including autosaving and cleanup.
    /// </summary>
    public static class EditorManagerHelper
    {
        /// <summary>
        /// Gets or sets the dictionary of editors associated with their corresponding folder entries.
        /// </summary>
        public static Dictionary<FolderEntryViewModel, EditorControl> Editors { get; set; } = [];

        /// <summary>
        /// Automatically saves all dirty editors and removes inactive editors that have not been saved for a specified duration.
        /// </summary>
        public static void AutoSaveEditors()
        {
            // List to store folder entries of editors that need to be removed.
            var editorsToRemove = new List<FolderEntryViewModel>();

            // Iterate through all editors in the dictionary.
            foreach (var kvp in Editors)
            {
                var editor = kvp.Value;

                // If the editor has unsaved changes, perform an autosave.
                if (editor.IsDirty)
                {
                    editor.AutoSave();
                }
                // If the editor is inactive and has not been saved for more than 10 minutes, mark it for removal.
                else if ((DateTime.UtcNow - editor.LastSaveTime).TotalMinutes > 10
                    && !kvp.Value.IsActive)
                {
                    editorsToRemove.Add(kvp.Key);
                }
            }

            // Dispose and remove editors marked for removal.
            foreach (var key in editorsToRemove)
            {
                Editors[key].Dispose();
                Editors.Remove(key);
            }
        }
    }
}
