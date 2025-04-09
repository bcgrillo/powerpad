using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using PowerPad.WinUI.Components;

namespace PowerPad.WinUI.Pages
{
    internal partial class WorkspacePage : Page, INavigationPage
    {
        public double NavigationWidth => WorkspaceControl.Visibility == Visibility.Visible ? WorkspaceControl.ActualWidth : 0;

        public WorkspacePage()
        {
            this.InitializeComponent();
        }

        public event EventHandler? NavigationVisibilityChanged;

        public void ToggleNavigationVisibility()
        {
            WorkspaceControl.Visibility = WorkspaceControl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            NavigationVisibilityChanged?.Invoke(this, null!);
        }

        private void WorkspaceControl_ItemInvoked(object sender, WorkspaceControlItemInvokedEventArgs e)
        {
            EditorManager.OpenFile(e.SelectedFile);
        }

        private void WorkspaceControl_VisibilityChanged(object sender, EventArgs e)
        {
            NavigationVisibilityChanged?.Invoke(this, null!);
        }
    }
}