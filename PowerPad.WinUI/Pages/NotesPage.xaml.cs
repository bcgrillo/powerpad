using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Markup;
using System;
using PowerPad.Core.Services;
using PowerPad.Core.Models;
using System.IO;
using PowerPad.WinUI.Components;

namespace PowerPad.WinUI.Pages
{
    internal sealed partial class NotesPage : Page, INavigationPage
    {
        public NotesPage()
        {
            this.InitializeComponent();
        }

        public event EventHandler<NavigationVisibilityChangedEventArgs>? NavigationVisibilityChanged;

        public void ToggleNavigationVisibility()
        {
            var isVisible = WorkspaceControl.Visibility == Visibility.Visible;

            WorkspaceControl.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;

            isVisible = !isVisible;

            NavigationVisibilityChanged?.Invoke(this, new NavigationVisibilityChangedEventArgs(isVisible ? WorkspaceControl.ActualWidth : 0));
        }

        private void WorkspaceControl_ItemInvoked(object sender, WorkspaceControlItemInvokedEventArgs e)
        {
            EditorManager.OpenFile(e.SelectedFile);
        }

        private void WorkspaceControl_VisibilityChanged(object sender, EventArgs e)
        {
            var isVisible = WorkspaceControl.Visibility == Visibility.Visible;

            NavigationVisibilityChanged?.Invoke(this, new NavigationVisibilityChangedEventArgs(isVisible ? WorkspaceControl.ActualWidth : 0));
        }
    }
}