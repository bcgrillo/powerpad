using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Components.Controls;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.Components
{
    public partial class SearchModelsResultRepeater : UserControl
    {
        public ObservableCollection<AIModelViewModel> Models
        {
            get => (ObservableCollection<AIModelViewModel>)GetValue(ModelsProperty);
            set => SetValue(ModelsProperty, value);
        }

        public static readonly DependencyProperty ModelsProperty =
            DependencyProperty.Register(nameof(Models), typeof(ObservableCollection<AIModelViewModel>), typeof(AvailableModelsRepeater), new(null));

        public bool SearchingFlag
        {
            get => (bool)GetValue(SearchingFlagProperty);
            set => SetValue(SearchingFlagProperty, value);
        }

        public static readonly DependencyProperty SearchingFlagProperty =
            DependencyProperty.Register(nameof(SearchingFlag), typeof(bool), typeof(SearchModelsResultRepeater), new(false));

        public bool SearchEmpty
        {
            get => (bool)GetValue(SearchEmptyProperty);
            set => SetValue(SearchEmptyProperty, value);
        }

        public static readonly DependencyProperty SearchEmptyProperty =
            DependencyProperty.Register(nameof(SearchEmpty), typeof(bool), typeof(AvailableModelsRepeater), new(false));

        public event EventHandler<AIModelClickEventArgs>? AddModelClick;
        public event EventHandler<ModelInfoViewerVisibilityEventArgs>? ModelInfoViewerVisibilityChanged;

        public SearchModelsResultRepeater()
        {
            this.InitializeComponent();
        }

        private void OnAddModelClick(object? sender, RoutedEventArgs __)
        {
            AddModelClick?.Invoke(sender, new((AIModelViewModel)((Button)sender!).Tag));
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs __)
        {
            var model = (AIModelViewModel)((HyperlinkButton)sender!).Tag;

            ModelInfoViewer.Show(model.CardName, model.InfoUrl!);
        }

        private void ModelInfoViewer_VisibilityChanged(object sender, ModelInfoViewerVisibilityEventArgs eventArgs)
        {
            MainContent.Visibility = eventArgs.IsVisible ? Visibility.Collapsed : Visibility.Visible;
            ModelInfoViewerVisibilityChanged?.Invoke(sender, eventArgs);
        }

        public void CloseModelInfoViewer() => ModelInfoViewer.Hide();
    }
}