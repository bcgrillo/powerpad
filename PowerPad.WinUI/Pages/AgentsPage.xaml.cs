using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Settings;
using Windows.UI;

namespace PowerPad.WinUI.Pages
{
    public partial class AgentsPage : DisposablePage, IToggleMenuPage
    {
        private readonly AgentsCollectionViewModel _agentsCollection;
        private readonly SettingsViewModel _settings;
        private bool _undoSelectionChange = false;

        private AgentViewModel? _selectedAgent;
        private AgentEditorControl? _editorControl;

        public double NavigationWidth => AgentsMenu.Visibility == Visibility.Visible ? TreeView.ActualWidth : 0;

        public AgentsPage()
        {
            this.InitializeComponent();

            _agentsCollection = App.Get<AgentsCollectionViewModel>();
            _settings = App.Get<SettingsViewModel>();

            foreach (var agent in _agentsCollection.Agents) agent.IsSelected = false;
        }

        public event EventHandler? NavigationVisibilityChanged;

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
                    if (_selectedAgent is not null) _selectedAgent.IsSelected = false;
                    if (invokedEntry is not null) invokedEntry.IsSelected = true;
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

        public void ToggleNavigationVisibility()
        {
            AgentsMenu.Visibility = AgentsMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            NavigationVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void NewAgentButton_Click(object _, RoutedEventArgs __)
        {
            var newIcon = _agentsCollection.GenerateIcon();
            var newAgent = new AgentViewModel(new Agent { Name = "Nuevo agente", Prompt = "Eres un agente que..."}, newIcon);

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

        private async void RenameFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            var entry = (AgentViewModel)((MenuFlyoutItem)sender).DataContext;

            var result = await DialogHelper.Imput(XamlRoot, "Renombrar", "Nuevo nombre:", entry.Name);

            if (result is not null) entry.Name = result;
        }

        private async void DeleteFlyoutItem_Click(object sender, RoutedEventArgs __)
        {
            var entry = (AgentViewModel)((MenuFlyoutItem)sender).DataContext;

            var result = await DialogHelper.Confirm(XamlRoot, "Eliminar", "¿Está seguro?");

            if (result == ContentDialogResult.Primary)
            {
                if (entry.IsSelected == true)
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

        private void MoreButton_Click(object sender, RoutedEventArgs __)
        {
            var button = sender as Button;

            var container = TreeView.ContainerFromItem(button?.DataContext) as TreeViewItem;

            if (container?.ContextFlyout is MenuFlyout flyout) flyout.ShowAt(button);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}