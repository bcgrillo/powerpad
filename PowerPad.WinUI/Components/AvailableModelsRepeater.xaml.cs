using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.AI;
using System;
using System.Collections.ObjectModel;

namespace PowerPad.WinUI.Components
{
    public sealed partial class AIModelsRepeater : UserControl
    {
        public ObservableCollection<AIModelViewModel> Models
        {
            get => (ObservableCollection<AIModelViewModel>)GetValue(ModelsProperty);
            set => SetValue(ModelsProperty, value);
        }

        public static readonly DependencyProperty ModelsProperty =
            DependencyProperty.Register(nameof(Models), typeof(ObservableCollection<AIModelViewModel>), typeof(AIModelsRepeater), new PropertyMetadata(null));

        public event EventHandler<AIModelClickEventArgs>? DeleteClick;
        public event EventHandler<AIModelClickEventArgs>? SetDefaultClick;

        public AIModelsRepeater()
        {
            this.InitializeComponent();
        }

        private void OnDeleteClick(object? sender, RoutedEventArgs _)
        {
            DeleteClick?.Invoke(sender, new AIModelClickEventArgs((AIModelViewModel)((MenuFlyoutItem)sender!).Tag));
        }

        private void OnSetDefaultClick(object? sender, RoutedEventArgs _)
        {
            SetDefaultClick?.Invoke(sender, new AIModelClickEventArgs((AIModelViewModel)((Button)sender!).Tag));
        }
    }

    public class AIModelClickEventArgs(AIModelViewModel model) : RoutedEventArgs
    {
        public AIModelViewModel Model { get; } = model;
    }
}
