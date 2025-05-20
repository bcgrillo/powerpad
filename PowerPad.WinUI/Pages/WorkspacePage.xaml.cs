using Microsoft.UI.Xaml;
using PowerPad.WinUI.Components;

namespace PowerPad.WinUI.Pages
{
    /// <summary>
    /// Represents the workspace page in the application, providing navigation and file management functionality.
    /// </summary>
    public partial class WorkspacePage : DisposablePage, IToggleMenuPage
    {
        /// <summary>
        /// Gets the width of the navigation pane based on its visibility.
        /// </summary>
        public double NavigationWidth => WorkspaceControl.Visibility == Visibility.Visible ? WorkspaceControl.ActualWidth : 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspacePage"/> class.
        /// </summary>
        public WorkspacePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Toggles the visibility of the navigation pane.
        /// </summary>
        public void ToggleNavigationVisibility()
        {
            WorkspaceControl.Visibility = WorkspaceControl.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Handles the event when an item in the workspace control is invoked.
        /// </summary>
        /// <param name="_">The source of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing the selected file.</param>
        private void WorkspaceControl_ItemInvoked(object _, WorkspaceControlItemInvokedEventArgs eventArgs)
        {
            EditorManager.OpenFile(eventArgs.SelectedFile);
        }

        /// <summary>
        /// Releases the resources used by the <see cref="WorkspacePage"/> class.
        /// </summary>
        /// <param name="disposing">A value indicating whether the method is called from the Dispose method.</param>
        protected override void Dispose(bool disposing)
        {
            // Nothing to dispose here
        }
    }
}