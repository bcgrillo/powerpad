using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Components.Controls;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.Components
{
    public partial class AvailableModelsRepeater : UserControl
    {
        public ObservableCollection<AIModelViewModel> Models
        {
            get => (ObservableCollection<AIModelViewModel>)GetValue(ModelsProperty);
            set => SetValue(ModelsProperty, value);
        }

        public static readonly DependencyProperty ModelsProperty =
            DependencyProperty.Register(nameof(Models), typeof(ObservableCollection<AIModelViewModel>), typeof(AvailableModelsRepeater), new(null));

        public bool ModelsEmpty
        {
            get => (bool)GetValue(ModelsEmptyProperty);
            set => SetValue(ModelsEmptyProperty, value);
        }

        public static readonly DependencyProperty ModelsEmptyProperty =
            DependencyProperty.Register(nameof(ModelsEmpty), typeof(bool), typeof(AvailableModelsRepeater), new(false));

        public event EventHandler<AIModelClickEventArgs>? DeleteClick;
        public event EventHandler<AIModelClickEventArgs>? SetDefaultClick;
        public event EventHandler? AddButtonClick;

        public event EventHandler<ModelInfoViewerVisibilityEventArgs>? ModelInfoViewerVisibilityChanged;

        public AvailableModelsRepeater()
        {
            this.InitializeComponent();
        }

        private void OnDeleteClick(object? sender, RoutedEventArgs __)
        {
            DeleteClick?.Invoke(sender, new((AIModelViewModel)((MenuFlyoutItem)sender!).Tag));
        }

        private void OnSetDefaultClick(object? sender, RoutedEventArgs __)
        {
            var model = (AIModelViewModel)((MenuFlyoutItem)sender!).Tag;

            if (!model.Enabled) model.Enabled = true;

            SetDefaultClick?.Invoke(sender, new(model));
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs __)
        {
            var model = (AIModelViewModel)((HyperlinkButton)sender!).Tag;

            ModelInfoViewer.Show(model.CardName, model.InfoUrl!);
        }

        private void ModelInfoViewer_VisibilityChanged(object sender, ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            ModelsScrollViewer.Visibility = eventArgs.IsVisible ? Visibility.Collapsed : Visibility.Visible;
            ModelInfoViewerVisibilityChanged?.Invoke(sender, eventArgs);
        }

        public void CloseModelInfoViewer() => ModelInfoViewer.Hide();

        private void AddModelsButton_Click(object _, RoutedEventArgs __)
        {
            AddButtonClick?.Invoke(this, EventArgs.Empty);
        }
    }

    public class AIModelClickEventArgs(AIModelViewModel model) : RoutedEventArgs
    {
        public AIModelViewModel Model { get; } = model;
    }
}
