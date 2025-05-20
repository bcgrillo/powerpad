using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Linq;

namespace PowerPad.WinUI.Pages
{
    /// <summary>
    /// Represents the page for managing AI agents, including their creation, editing, and deletion.
    /// </summary>
    public partial class AgentsPage : DisposablePage, IToggleMenuPage
    {
        private readonly AgentsCollectionViewModel _agentsCollection;
        private bool _undoSelectionChange = false;

        private AgentViewModel? _selectedAgent;
        private AgentEditorControl? _editorControl;

        /// <summary>
        /// Gets the width of the navigation menu based on its visibility.
        /// </summary>
        public double NavigationWidth => AgentsMenu.Visibility == Visibility.Visible ? TreeView.ActualWidth : 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentsPage"/> class.
        /// </summary>
        public AgentsPage()
        {
            this.InitializeComponent();

            _agentsCollection = App.Get<AgentsCollectionViewModel>();

            foreach (var agent in _agentsCollection.Agents) agent.IsSelected = false;
        }

        /// <summary>
        /// Toggles the visibility of the navigation menu.
        /// </summary>
        public void ToggleNavigationVisibility()
        {
            AgentsMenu.Visibility = AgentsMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Handles the selection change event in the TreeView.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments containing the selection details.</param>
        private async void TreeView_SelectionChanged(TreeView _, TreeViewSelectionChangedEventArgs eventArgs)
        {
            var invokedEntry = (AgentViewModel?)eventArgs.AddedItems.FirstOrDefault();

            if (!_undoSelectionChange)
            {
                bool cancel = false;

                if (_editorControl is not null)
                {
                    var result = await _editorControl.ConfirmClose();

                    if (!result) cancel = true;
                }

                if (cancel)
                {
                    _undoSelectionChange = true;
                    TreeView.SelectedItem = _selectedAgent;
                    _undoSelectionChange = false;
                }
                else
                {
                    _selectedAgent?.IsSelected = false;
                    invokedEntry?.IsSelected = true;
                    _selectedAgent = invokedEntry;

                    if (_editorControl is not null)
                    {
                        AgentEditorContent.Children.Clear();
                        _editorControl.Dispose();
                        _editorControl = null;
                    }

                    if (invokedEntry is not null)
                    {
                        _editorControl = new AgentEditorControl(invokedEntry, XamlRoot);
                        AgentEditorContent.Children.Add(_editorControl);
                    }
                }

                UpdateLandingVisibility(showLanding: invokedEntry is null);
            }
        }

        /// <summary>
        /// Updates the visibility of the landing page or editor content.
        /// </summary>
        /// <param name="showLanding">A boolean indicating whether to show the landing page.</param>
        private void UpdateLandingVisibility(bool showLanding)
        {
            if (showLanding)
            {
                AgentLanding.Visibility = Visibility.Visible;
                AgentEditorContent.Visibility = Visibility.Collapsed;
            }
            else
            {
                AgentLanding.Visibility = Visibility.Collapsed;
                AgentEditorContent.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handles the click event for creating a new agent.
        /// </summary>
        private void NewAgentButton_Click(object _, RoutedEventArgs __)
        {
            var newIcon = _agentsCollection.GenerateIcon();
            var newAgent = new AgentViewModel(new Agent { Name = "Nuevo agente", Prompt = "Eres un agente amable y resolutivo." }, newIcon);

            _agentsCollection.Agents.Add(newAgent);

            DispatcherQueue.TryEnqueue(() =>
            {
                TreeView.SelectedItem = newAgent;

                var container = (TreeViewItem)TreeView.ContainerFromItem(newAgent);
                container?.StartBringIntoView(new BringIntoViewOptions
                {
                    AnimationDesired = true
                });
            });
        }

        /// <summary>
        /// Handles the click event for renaming an agent.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private async void RenameFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            var entry = (AgentViewModel)((MenuFlyoutItem)sender).DataContext;

            var result = await DialogHelper.Input(XamlRoot, "Renombrar", "Nuevo nombre:", entry.Name);

            if (result is not null) entry.Name = result;
        }

        /// <summary>
        /// Handles the click event for deleting an agent.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private async void DeleteFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            var entry = (AgentViewModel)((MenuFlyoutItem)sender).DataContext;

            var result = await DialogHelper.Confirm(XamlRoot, "Eliminar", "¿Está seguro?");

            if (result == ContentDialogResult.Primary)
            {
                if (entry.IsSelected)
                {
                    TreeView.SelectedItem = null;

                    if (_editorControl is not null)
                    {
                        AgentEditorContent.Children.Clear();
                        _editorControl.Dispose();
                        _editorControl = null;
                        _selectedAgent = null;
                    }

                    UpdateLandingVisibility(showLanding: true);
                }

                _agentsCollection.Agents.Remove(entry);
            }
        }

        /// <summary>
        /// Handles the click event for showing the context menu of an agent.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private void MoreButton_Click(object sender, RoutedEventArgs __)
        {
            var button = sender as Button;

            var container = TreeView.ContainerFromItem(button?.DataContext) as TreeViewItem;

            if (container?.ContextFlyout is MenuFlyout flyout) flyout.ShowAt(button);
        }

        /// <summary>
        /// Handles the drop completed event for a TreeView item.
        /// </summary>
        /// <param name="sender">The UI element that triggered the event.</param>
        /// <param name="__">The event arguments (not used).</param>
        private void TreeViewItem_DropCompleted(UIElement sender, DropCompletedEventArgs __)
        {
            TreeView.SelectedItem ??= (AgentViewModel?)((TreeViewItem)sender).DataContext;
        }

        /// <summary>
        /// Disposes of resources used by the page.
        /// </summary>
        /// <param name="disposing">A boolean indicating whether to dispose managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Nothing to dispose in this case
        }
    }
}