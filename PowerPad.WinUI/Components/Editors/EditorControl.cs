using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Contracts;
using System;

namespace PowerPad.WinUI.Components.Editors
{
    public abstract class EditorControl : UserControl, IEditorContract, IDisposable
    {
        public abstract string GetContent();
        public abstract void SetContent(string content);
        public abstract bool IsDirty { get; }
        public abstract void AutoSave();
        public abstract DateTime LastSaveTime { get; }
        public abstract void Dispose();
    }
}