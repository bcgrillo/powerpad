using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Agents;

namespace PowerPad.WinUI.Pages
{
    public partial class AgentsPage : DisposablePage, INavigationPage
    {
        private readonly AgentsCollectionViewModel _agentsCollection;

        public double NavigationWidth => NavView.IsPaneVisible ? NavView.OpenPaneLength : 0;

        public AgentsPage()
        {
            this.InitializeComponent();

            _agentsCollection = App.Get<AgentsCollectionViewModel>();
        }

        public event EventHandler? NavigationVisibilityChanged;

        private void NavView_SelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
        {
        }

        private void NavigateToPage(ModelProvider _)
        {
        }

        public void ToggleNavigationVisibility()
        {
            NavView.IsPaneVisible = !NavView.IsPaneVisible;

            NavigationVisibilityChanged?.Invoke(this, null!);
        }

        private void NavView_Loaded(object _, RoutedEventArgs __)
        {
        }

        private void HideMenuBtn_Click(object _, RoutedEventArgs __) => ToggleNavigationVisibility();

        public override void Dispose()
        {
        }
    }
}