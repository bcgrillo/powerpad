using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaSharp.Models;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
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

        public SettingsPage()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();

            _settings.General.OllamaConfig.PropertyChanged += TestOllama;
            _settings.General.AzureAIConfig.PropertyChanged += TestAzureAI;
            _settings.General.OpenAIConfig.PropertyChanged += TestOpenAI;

            SetModelsMenu(null, null!);
            _settings.Models.AvailableModels.CollectionChanged += SetModelsMenu;

            LightThemeRadioButton.IsChecked = _settings.General.AppTheme == ApplicationTheme.Light;
            DarkThemeRadioButton.IsChecked = _settings.General.AppTheme == ApplicationTheme.Dark;
            SystemThemeRadioButton.IsChecked = _settings.General.AppTheme is null;
        }

        private async void StartOllama_Click(object _, RoutedEventArgs __)
        {
            var protectedCursorAux = ProtectedCursor;

            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                await App.Get<IOllamaService>().Start();
                await _settings.UpdateOllamaStatus();
            }
            catch
            {
                OllamaInfoBar.IsOpen = true;
                OllamaInfoBar.Message = "No se ha podido iniciar Ollama.";
                OllamaInfoBar.Severity = InfoBarSeverity.Error;
            }
            finally
            {
                ProtectedCursor = protectedCursorAux;
            }
        }

        private async void StopOllama_Click(object _, RoutedEventArgs __)
        {
            var protectedCursorAux = ProtectedCursor;

            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                await App.Get<IOllamaService>().Stop();
                await _settings.UpdateOllamaStatus();
            }
            catch
            {
                OllamaInfoBar.IsOpen = true;
                OllamaInfoBar.Message = "No se ha podido detener Ollama.";
                OllamaInfoBar.Severity = InfoBarSeverity.Error;
            }
            finally
            {
                ProtectedCursor = protectedCursorAux;
            }
        }

        private async void TestOllama(object? _, PropertyChangedEventArgs __)
        {
            var protectedCursorAux = ProtectedCursor;

            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                var result = await App.Get<IOllamaService>().TestConection();

                if (!result.Success)
                {
                    OllamaInfoBar.IsOpen = true;
                    OllamaInfoBar.Message = result.ErrorMessage;
                    OllamaInfoBar.Severity = InfoBarSeverity.Error;
                }
                else
                {
                    OllamaInfoBar.IsOpen = false;
                }
            }
            finally
            {
                ProtectedCursor = protectedCursorAux;
            }
        }

        private async void TestAzureAI(object? _, PropertyChangedEventArgs __)
        {
            var protectedCursorAux = ProtectedCursor;

            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                var result = await App.Get<IAzureAIService>().TestConection();

                if (!result.Success)
                {
                    AzureAIInfoBar.IsOpen = true;
                    AzureAIInfoBar.Message = result.ErrorMessage;
                    AzureAIInfoBar.Severity = InfoBarSeverity.Error;
                }
                else
                {
                    AzureAIInfoBar.IsOpen = false;
                }
            }
            finally
            {
                ProtectedCursor = protectedCursorAux;
            }
        }

        private async void TestOpenAI(object? _, PropertyChangedEventArgs __)
        {
            var protectedCursorAux = ProtectedCursor;

            try
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Wait);

                var result = await App.Get<IOpenAIService>().TestConection();

                if (!result.Success)
                {
                    OpenAIInfoBar.IsOpen = true;
                    OpenAIInfoBar.Message = result.ErrorMessage;
                    OpenAIInfoBar.Severity = InfoBarSeverity.Error;
                }
                else
                {
                    OpenAIInfoBar.IsOpen = false;
                }
            }
            finally
            {
                ProtectedCursor = protectedCursorAux;
            }
        }

        private void SetModelsMenu(object? _, NotifyCollectionChangedEventArgs __)
        {
            DefaultModelFlyoutMenu.Items.Clear();

            var availableProviders = _settings.General.GetAvailableModelProviders();

            foreach (var provider in availableProviders)
            {
                var elementAdded = false;

                foreach (var item in _settings.Models.AvailableModels.Where(m => m.ModelProvider == provider && m.Enabled))
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
                FakeButton.Visibility = Visibility.Collapsed;
                DefaultModelButton.Visibility = Visibility.Visible;
            }
            else
            {
                DefaultModelCard.IsEnabled = false;
                DefaultParameterCard.IsEnabled = false;
                FakeButton.Visibility = Visibility.Visible;
                DefaultModelButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SetModelItem_Click(object sender, RoutedEventArgs _)
        {
            ((RadioMenuFlyoutItem)sender).IsChecked = true;
            _settings.Models.DefaultModel = (AIModelViewModel?)((RadioMenuFlyoutItem)sender).Tag;
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
                    _settings.General.OllamaConfig.PropertyChanged -= TestOllama;
                    _settings.General.AzureAIConfig.PropertyChanged -= TestAzureAI;
                    _settings.General.OpenAIConfig.PropertyChanged -= TestOpenAI;
                    _settings.Models.AvailableModels.CollectionChanged -= SetModelsMenu;
                }

                _disposed = true;
            }
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

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = ((RadioButton)sender);
            var newThemeChoice = radioButton.Tag as ApplicationTheme?;

            if (radioButton.IsChecked == true && _settings.General.AppTheme != newThemeChoice)
            {
                _settings.General.AppTheme = newThemeChoice;

                ThemeInfoBar.IsOpen = true;
                ThemeInfoBar.Message = "Reinicia la aplicación para aplicar los cambios.";
                ThemeInfoBar.Severity = InfoBarSeverity.Informational;
            }
        }
    }
}