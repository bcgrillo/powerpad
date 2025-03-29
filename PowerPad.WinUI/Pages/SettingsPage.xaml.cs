using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.Settings;

namespace PowerPad.WinUI.Pages
{
    internal sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _settings;

        public SettingsPage()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();
        }
    }
}