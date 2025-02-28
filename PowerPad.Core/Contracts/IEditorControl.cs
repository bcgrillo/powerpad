using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Components.Editors
{
    public interface IEditorControl
    {
        string GetContent();
        void SetContent(string content);
    }
}
