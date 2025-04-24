using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Agents;

namespace PowerPad.WinUI.Pages
{
    public partial class AgentsPage : DisposablePage, IToggleMenuPage
    {
        private readonly AgentsCollectionViewModel _agentsCollection;

        public double NavigationWidth => AgentsMenu.Visibility == Visibility.Visible ? TreeView.ActualWidth : 0;

        public AgentsPage()
        {
            this.InitializeComponent();

            _agentsCollection = App.Get<AgentsCollectionViewModel>();
        }

        public event EventHandler? NavigationVisibilityChanged;

        private void TreeView_ItemInvoked(TreeView _, TreeViewItemInvokedEventArgs args)
        {
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
        }

        public override void Dispose()
        {
        }

        //private void TreeView_DragItemsCompleted(TreeView _, TreeViewDragItemsCompletedEventArgs args)
        //{
        //    if (args.DropResult == DataPackageOperation.Move && args.Items.Count == 1 && args.Items[0] is AgentViewModel agent)
        //    {
        //        _agentsCollection.Agents.Move(_agentsCollection.Agents.IndexOf(agent), TreeView.);
        //    }
        //}
    }
}