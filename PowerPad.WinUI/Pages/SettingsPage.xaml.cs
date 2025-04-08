using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Linq;

namespace PowerPad.WinUI.Pages
{
    internal sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _settings;

        public SettingsPage()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();

            _settings.General.AzureAIConfig.PropertyChanged += (s, e) => TestAzureAI();
            _settings.General.OpenAIConfig.PropertyChanged += (s, e) => TestOpenAI();

            SetModelsMenu();
            _settings.Models.AvailableModels.CollectionChanged += (s, e) => SetModelsMenu();
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
            catch(Exception)
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
            catch (Exception)
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

        private async void TestAzureAI()
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
            }
            finally
            {
                ProtectedCursor = protectedCursorAux;
            }
        }

        private async void TestOpenAI()
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
            }
            finally
            {
                ProtectedCursor = protectedCursorAux;
            }
        }

        private void SetModelsMenu()
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

            DefaultModelFlyoutMenu.Items.RemoveAt(DefaultModelFlyoutMenu.Items.Count - 1);
        }

        private void SetModelItem_Click(object sender, RoutedEventArgs _)
        {
            ((RadioMenuFlyoutItem)sender).IsChecked = true;
            _settings.Models.DefaultModel = (AIModelViewModel?)((RadioMenuFlyoutItem)sender).Tag;
        }
    }
}