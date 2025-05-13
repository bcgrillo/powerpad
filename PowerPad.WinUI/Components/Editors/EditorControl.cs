using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Contracts;
using System;

namespace PowerPad.WinUI.Components.Editors
{
    /// <summary>
    /// Represents an abstract base class for editor controls, providing core functionality
    /// and contract implementation for content management and editor state.
    /// </summary>
    public abstract class EditorControl : UserControl, IEditorContract, IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the editor is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether the editor's content has unsaved changes.
        /// </summary>
        public abstract bool IsDirty { get; }

        /// <summary>
        /// Gets the timestamp of the last save operation.
        /// </summary>
        public abstract DateTime LastSaveTime { get; }

        /// <summary>
        /// Retrieves the content of the editor.
        /// </summary>
        /// <param name="plainText">If true, returns the content as plain text without formatting.</param>
        /// <returns>The content of the editor as a string.</returns>
        public abstract string GetContent(bool plainText = false);

        /// <summary>
        /// Sets the content of the editor.
        /// </summary>
        /// <param name="content">The content to set in the editor.</param>
        public abstract void SetContent(string content);

        /// <summary>
        /// Sets focus to the editor control.
        /// </summary>
        public abstract void SetFocus();

        /// <summary>
        /// Automatically saves the current content of the editor.
        /// </summary>
        public abstract void AutoSave();

        /// <summary>
        /// Calculates the number of words in the editor's content.
        /// </summary>
        /// <returns>The word count as an integer.</returns>
        public abstract int WordCount();

        /// <summary>
        /// Releases the resources used by the editor control.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the editor control and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">If true, releases both managed and unmanaged resources; otherwise, releases only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);
    }
}