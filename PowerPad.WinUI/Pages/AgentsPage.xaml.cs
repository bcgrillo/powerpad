using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.FileSystem;

namespace PowerPad.WinUI.Pages
{
    public partial class AgentsPage : DisposablePage, IToggleMenuPage
    {
        private readonly AgentsCollectionViewModel _agentsCollection;
        private AgentViewModel? _selectedAgent;
        private AgentEditorControl? _editorControl;

        public double NavigationWidth => AgentsMenu.Visibility == Visibility.Visible ? TreeView.ActualWidth : 0;

        public AgentsPage()
        {
            this.InitializeComponent();

            _agentsCollection = App.Get<AgentsCollectionViewModel>();
        }

        public event EventHandler? NavigationVisibilityChanged;

        private async void TreeView_ItemInvoked(TreeView _, TreeViewItemInvokedEventArgs eventArgs)
        {
            var invokedEntry = (AgentViewModel)eventArgs.InvokedItem;
            bool cancel = false;

            if (_editorControl is not null)
            {
                var result = await _editorControl.ConfirmClose(XamlRoot);

                if (result)
                {
                    AgentEditorContent.Content = null;
                    _editorControl.Dispose();
                    _editorControl = null;
                    _selectedAgent = null;
                }
                else cancel = true;
            }
            else UpdateLandingVisibility(showLanding: false);

            if (cancel)
            {
                TreeView.SelectedItem = _selectedAgent;
            }
            else
            {
                _selectedAgent = invokedEntry;
                _editorControl = new AgentEditorControl(invokedEntry);
                AgentEditorContent.Content = _editorControl;
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

        private void NavigateToPage(ModelProvider _)
        {
        }

        public void ToggleNavigationVisibility()
        {
            AgentsMenu.Visibility = AgentsMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            NavigationVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void NewAgentButton_Click(object _, RoutedEventArgs __)
        {
            var newAgent = new AgentViewModel(new Agent { Name = "Nuevo agente", Prompt = "Eres un agente que..."});

            _agentsCollection.Agents.Add(newAgent);

            TreeView.SelectedItem = newAgent;
        }

        public override void Dispose()
        {
        }

        //private void TreeView_DragItemsCompleted(TreeView _, TreeViewDragItemsCompletedEventArgs eventArgs)
        //{
        //    if (args.DropResult == DataPackageOperation.Move && eventArgs.Items.Count == 1 && eventArgs.Items[0] is AgentViewModel agent)
        //    {
        //        _agentsCollection.Agents.Move(_agentsCollection.Agents.IndexOf(agent), TreeView.);
        //    }
        //}
    }
}