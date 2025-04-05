using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Pages.Providers;
using PowerPad.WinUI.ViewModels.Settings;
using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.Pages
{
    public sealed partial class AgentsPage : Page, INavigationPage
    {

        public double NavigationWidth => NavView.IsPaneVisible ? NavView.OpenPaneLength : 0;

        private readonly SettingsViewModel _settings;

        public AgentsPage()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();
        }

        public event EventHandler? NavigationVisibilityChanged;

        private void NavView_SelectionChanged(NavigationView _, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not null) 
                NavigateToPage((ModelProvider)((NavigationViewItem)args.SelectedItem)!.Tag);
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
    }
}