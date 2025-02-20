using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using WinUIEditor;

namespace PowerPad.WinUI.Components
{
    public sealed partial class EditorView : UserControl
    {
        public EditorView()
        {
            this.InitializeComponent();
        }

        public string GetContent()
        {
            return MyEditor.Editor.GetText(MyEditor.Editor.Length);
        }

        public void SetContent(string content)
        {
            MyEditor.Editor.SetText(content);
        }
    }
}
