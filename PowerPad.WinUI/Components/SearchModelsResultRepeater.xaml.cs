using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.Components
{
    public sealed partial class SearchModelsResultRepeater : UserControl
    {
        public ObservableCollection<AIModelViewModel> Models
        {
            get => (ObservableCollection<AIModelViewModel>)GetValue(ModelsProperty);
            set => SetValue(ModelsProperty, value);
        }

        public static readonly DependencyProperty ModelsProperty =
            DependencyProperty.Register(nameof(Models), typeof(ObservableCollection<AIModelViewModel>), typeof(AIModelsRepeater), new(null));

        public bool SearchingFlag
        {
            get => (bool)GetValue(SearchingFlagProperty);
            set => SetValue(SearchingFlagProperty, value);
        }

        public static readonly DependencyProperty SearchingFlagProperty =
            DependencyProperty.Register(nameof(SearchingFlag), typeof(bool), typeof(SearchModelsResultRepeater), new(false));


        public event EventHandler<AIModelClickEventArgs>? AddModelClick;

        public SearchModelsResultRepeater()
        {
            this.InitializeComponent();
        }

        private void OnAddModelClick(object? sender, RoutedEventArgs e)
        {
            AddModelClick?.Invoke(sender, new((AIModelViewModel)((Button)sender!).Tag));
        }
    }
}