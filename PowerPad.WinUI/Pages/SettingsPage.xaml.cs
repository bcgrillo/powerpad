using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.Dialogs;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace PowerPad.WinUI.Pages
{
    public partial class SettingsPage : DisposablePage
    {
        private readonly SettingsViewModel _settings;
        private readonly InputCursor _defaultCursor;
        private readonly ApplicationTheme? _originalTheme;

        public SettingsPage()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();
            _defaultCursor = ProtectedCursor;
            _originalTheme = _settings.General.AppTheme;

            _settings.General.OllamaConfig.ConfigChanged += TestOllama;
            _settings.General.AzureAIConfig.ConfigChanged += TestAzureAI;
            _settings.General.OpenAIConfig.ConfigChanged += TestOpenAI;
            _settings.General.ServiceEnablementChanged += TestEnabledService;

            SetModelsMenu(null, null);
            _settings.General.ProviderAvaibilityChanged += SetModelsMenu;
            _settings.Models.ModelAvaibilityChanged += SetModelsMenu;

            LightThemeRadioButton.IsChecked = _settings.General.AppTheme == ApplicationTheme.Light;
            DarkThemeRadioButton.IsChecked = _settings.General.AppTheme == ApplicationTheme.Dark;
            SystemThemeRadioButton.IsChecked = _settings.General.AppTheme is null;
            LightThemeRadioButton.Checked += ThemeRadioButton_Checked;
            DarkThemeRadioButton.Checked += ThemeRadioButton_Checked;
            SystemThemeRadioButton.Checked += ThemeRadioButton_Checked;

            OllamaModelsExpander.IsExpanded = _settings.General.OllamaEnabled 
                && (_settings.General.OllamaConfig.ServiceStatus == ServiceStatus.Error || _settings.General.OllamaConfig.ServiceStatus == ServiceStatus.NotFound);
            AzureAIModelsExpander.IsExpanded = _settings.General.AzureAIEnabled && _settings.General.AzureAIConfig.ServiceStatus == ServiceStatus.Error;
            OpenAIModelsExpander.IsExpanded = _settings.General.OpenAIEnabled && _settings.General.OpenAIConfig.ServiceStatus == ServiceStatus.Error;

            //Disable providers if not configured
            Unloaded += (s, e) =>
            {
                if (_settings.General.OllamaEnabled && string.IsNullOrEmpty(_settings.General.OllamaConfig.BaseUrl))
                    _settings.General.OllamaEnabled = false;

                if (_settings.General.AzureAIEnabled && string.IsNullOrEmpty(_settings.General.AzureAIConfig.BaseUrl))
                    _settings.General.AzureAIEnabled = false;

                if (_settings.General.OpenAIEnabled && string.IsNullOrEmpty(_settings.General.OpenAIConfig.BaseUrl))
                    _settings.General.OpenAIEnabled = false;
            };
        }

        private async void StartOllama_Click(object _, RoutedEventArgs __)
        {
            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                await App.Get<IOllamaService>().Start();
                await _settings.General.OllamaConfig.TestConnection(App.Get<IAIService>(ModelProvider.Ollama));
            }
            catch
            {
                _settings.General.OllamaConfig.SetErrorStatus("No se ha podido iniciar Ollama.");
            }
            finally
            {
                ProtectedCursor = _defaultCursor;
            }
        }

        private async void InstallOllama_Click(object _, RoutedEventArgs __)
        {
            var ollamaInstallationDialog = await OllamaDownloadHelper.ShowAsync(Content.XamlRoot);

            switch (ollamaInstallationDialog)
            {
                case ContentDialogResult.Primary:
                    await _settings.TestConnections();
                    break;
                case ContentDialogResult.Secondary:
                    break;
                default:
                    _settings.General.OllamaEnabled = false;
                    break;
            }
        }

        private async void StopOllama_Click(object _, RoutedEventArgs __)
        {
            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                await App.Get<IOllamaService>().Stop();
                await _settings.General.OllamaConfig.TestConnection(App.Get<IAIService>(ModelProvider.Ollama));
            }
            catch
            {
                _settings.General.OllamaConfig.SetErrorStatus("No se ha podido detener Ollama.");
            }
            finally
            {
                ProtectedCursor = _defaultCursor;
            }
        }

        private async void TestOllama(object? _, PropertyChangedEventArgs? __)
        {
            if (string.IsNullOrEmpty(_settings.General.OllamaConfig.BaseUrl)) return;

            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                await _settings.General.OllamaConfig.TestConnection(App.Get<IAIService>(ModelProvider.Ollama));

            }
            finally
            {
                ProtectedCursor = _defaultCursor;
            }
        }

        private async void TestAzureAI(object? _, PropertyChangedEventArgs? __)
        {
            if (string.IsNullOrEmpty(_settings.General.AzureAIConfig.BaseUrl)
                || string.IsNullOrEmpty(_settings.General.AzureAIConfig.Key)) return;

            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                await _settings.General.AzureAIConfig.TestConnection(App.Get<IAIService>(ModelProvider.GitHub));
            }
            finally
            {
                ProtectedCursor = _defaultCursor;
            }
        }

        private async void TestOpenAI(object? _, PropertyChangedEventArgs? __)
        {
            if (string.IsNullOrEmpty(_settings.General.OpenAIConfig.BaseUrl)
                || string.IsNullOrEmpty(_settings.General.OpenAIConfig.Key)) return;

            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                await _settings.General.OpenAIConfig.TestConnection(App.Get<IAIService>(ModelProvider.OpenAI));
            }
            finally
            {
                ProtectedCursor = _defaultCursor;
            }
        }

        private void SetModelsMenu(object? _, EventArgs? __)
        {
            DefaultModelFlyoutMenu.Items.Clear();

            var availableProviders = _settings.General.AvailableProviders.OrderBy(p => p);

            foreach (var provider in availableProviders)
            {
                var elementAdded = false;

                foreach (var item in _settings.Models.AvailableModels
                    .Where(m => m.ModelProvider == provider && m.Enabled)
                    .OrderBy(m => m.Name))
                {
                    var menuItem = new RadioMenuFlyoutItem
                    {
                        Text = item.CardName,
                        Tag = item,
                        Icon = new ImageIcon() { Source = provider.GetIcon() }
                    };

                    if (item == _settings.Models.DefaultModel) menuItem.IsChecked = true;

                    DefaultModelFlyoutMenu.Items.Add(menuItem);

                    menuItem.Click += SetModelItem_Click;

                    elementAdded = true;
                }

                if (elementAdded) DefaultModelFlyoutMenu.Items.Add(new MenuFlyoutSeparator());
            }

            if (DefaultModelFlyoutMenu.Items.Any())
            {
                DefaultModelFlyoutMenu.Items.RemoveAt(DefaultModelFlyoutMenu.Items.Count - 1);

                DefaultModelCard.IsEnabled = true;
                DefaultParameterCard.IsEnabled = true;
                AgentConfigCard.IsEnabled = true;
                FakeButton.Visibility = Visibility.Collapsed;
                DefaultModelButton.Visibility = Visibility.Visible;
            }
            else
            {
                DefaultModelCard.IsEnabled = false;
                DefaultParameterCard.IsEnabled = false;
                AgentConfigCard.IsEnabled = false;
                FakeButton.Visibility = Visibility.Visible;
                DefaultModelButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SetModelItem_Click(object sender, RoutedEventArgs __)
        {
            ((RadioMenuFlyoutItem)sender).IsChecked = true;
            _settings.Models.DefaultModel = (AIModelViewModel?)((RadioMenuFlyoutItem)sender).Tag;
        }

        private void DefaultModelButton_Click(object _, RoutedEventArgs __)
        {
            if (_settings.Models.DefaultModel is not null)
            {
                var menuItem = (RadioMenuFlyoutItem?)DefaultModelFlyoutMenu.Items.FirstOrDefault(i => i.Tag as AIModelViewModel == _settings.Models.DefaultModel);

                if (menuItem is not null && !menuItem.IsChecked) menuItem.IsChecked = true;
            }
        }

        private void ThemeInfoBarButton_Click(object _, RoutedEventArgs __)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
            Application.Current.Exit();
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs __)
        {
            var radioButton = ((RadioButton)sender);
            var newThemeChoice = radioButton.Tag as ApplicationTheme?;

            if (radioButton.IsChecked == true && _settings.General.AppTheme != newThemeChoice)
            {
                _settings.General.AppTheme = newThemeChoice;

                ThemeInfoBar.Visibility = _originalTheme != newThemeChoice ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OllamaModelsExpander_Toggled(object _, RoutedEventArgs __)
        {
            if (!_settings.General.OllamaEnabled)
            {
                if (string.IsNullOrEmpty(_settings.General.OllamaConfig.BaseUrl))
                {
                    OllamaModelsExpander.IsExpanded = true;
                    OllamaUrlTextBox.Focus(FocusState.Keyboard);
                    OllamaUrlTextBox.EnterEditMode();
                }
            }
        }

        private void AzureAIModelsExpander_Toggled(object _, RoutedEventArgs __)
        {
            if (!_settings.General.AzureAIEnabled)
            {
                if (string.IsNullOrEmpty(_settings.General.AzureAIConfig.BaseUrl))
                {
                    AzureAIModelsExpander.IsExpanded = true;
                    AzureAIUrlTextBox.Focus(FocusState.Keyboard);
                    AzureAIUrlTextBox.EnterEditMode();
                    AzureAIKeyTextBox.EnterEditMode();
                }
            }
        }

        private void OpenAIModelsExpander_Toggled(object _, RoutedEventArgs __)
        {
            if (!_settings.General.OpenAIEnabled)
            {
                if (string.IsNullOrEmpty(_settings.General.OpenAIConfig.BaseUrl))
                {
                    OpenAIModelsExpander.IsExpanded = true;
                    OpenAIUrlTextBox.Focus(FocusState.Keyboard);
                    OpenAIUrlTextBox.EnterEditMode();
                    OpenAIKeyTextBox.EnterEditMode();
                }
            }
        }

        private void TestEnabledService(object? sender, EventArgs _)
        {
            var config = (AIServiceConfigViewModel)sender!;

            if (config == _settings.General.OllamaConfig && _settings.General.OllamaEnabled) TestOllama(null, null);
            else if (config == _settings.General.AzureAIConfig && _settings.General.AzureAIEnabled) TestAzureAI(null, null);
            else if (config == _settings.General.OpenAIConfig && _settings.General.OpenAIEnabled) TestOpenAI(null, null);
        }

        private void RetryButton_Click(object sender, RoutedEventArgs __)
        {
            var config = (Button)sender!;

            if (config == OllamaRetryButton) TestOllama(null, null);
            else if (config == AzureAIRetryButton) TestAzureAI(null, null);
            else if (config == OpenAIRetryButton) TestOpenAI(null, null);
        }

        ~SettingsPage()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _settings.General.OllamaConfig.ConfigChanged -= TestOllama;
                    _settings.General.AzureAIConfig.ConfigChanged -= TestAzureAI;
                    _settings.General.OpenAIConfig.ConfigChanged -= TestOpenAI;
                    _settings.General.ServiceEnablementChanged -= TestEnabledService;
                    _settings.General.ProviderAvaibilityChanged -= SetModelsMenu;
                    _settings.Models.ModelAvaibilityChanged -= SetModelsMenu;
                }

                _disposed = true;
            }
        }
    }
}