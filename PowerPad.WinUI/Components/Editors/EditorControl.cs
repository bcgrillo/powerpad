using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Components.Editors
{
    public abstract class EditorControl : UserControl
    {
        public FileItem FileItem { get; protected set; }

        public EditorControl(FileItem fileItem)
        {
            FileItem = fileItem;
        }

        public abstract void AutoSave();
        public abstract void Save();
    }
}
