using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.Helpers;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Components.Controls
{
    public sealed partial class ModelSelector : UserControl, IDisposable
    {
        private const double DEBOURCE_INTERVAL = 200;

        private readonly SettingsViewModel _settings;
        private readonly DispatcherTimer _debounceTimer;

        public bool ShowDefaultOnButtonContent
        {
            get => (bool)GetValue(ShowDefaultOnButtonContentProperty);
            set
            {
                SetValue(ShowDefaultOnButtonContentProperty, value);
                RegenerateFlyoutMenu();
            }
        }

        public static readonly DependencyProperty ShowDefaultOnButtonContentProperty =
            DependencyProperty.Register(nameof(ShowDefaultOnButtonContent), typeof(bool), typeof(AgentSelector), new(false));

        public AIModelViewModel? SelectedModel
        {
            get;
            private set
            {
                if (field != value)
                {
                    field = value;
                    SelectedModelChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? SelectedModelChanged;

        public ModelSelector()
        {
            this.InitializeComponent();

            _settings = App.Get<SettingsViewModel>();

            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DEBOURCE_INTERVAL)
            };
            _debounceTimer.Tick += DebounceTimer_Tick;
            _debounceTimer.Stop();
        }

        public void Initialize(AIModelViewModel? model)
        {
            SelectedModel = model;
            RegenerateFlyoutMenu();

            _settings.General.ProviderAvaibilityChanged += Models_PropertyChanged;
            _settings.Models.ModelAvaibilityChanged += Models_PropertyChanged;
            _settings.Models.DefaultModelChanged += DefaultModel_Changed;
        }

        public void UpdateEnabledLayout(bool newValue)
        {
            ModelIcon.UpdateEnabledLayout(newValue);
            UpdateChekedItemMenu();
        }

        private void Select(AIModelViewModel? model)
        {
            if (model is null)
            {
                SelectedModel = null;
            }
            else
            {
                var menuItem = (RadioMenuFlyoutItem?)ModelFlyoutMenu.Items.FirstOrDefault(i => i.Tag as AIModelViewModel == model);

                if (menuItem is not null)
                {
                    SelectedModel = model;
                }
                else
                {
                    SelectedModel = null;
                }
            }

            UpdateChekedItemMenu();
            UpdateButtonContent();
        }

        private async void UpdateChekedItemMenu()
        {
            await Task.Delay(100);

            foreach (RadioMenuFlyoutItem item in ModelFlyoutMenu.Items.OfType<RadioMenuFlyoutItem>())
                item.IsChecked = false;

            var menuItem = SelectedModel is null
                ? (RadioMenuFlyoutItem?)ModelFlyoutMenu.Items.FirstOrDefault()
                : (RadioMenuFlyoutItem?)ModelFlyoutMenu.Items.FirstOrDefault(i => i.Tag as AIModelViewModel == SelectedModel);

            if (menuItem is not null) menuItem.IsChecked = true;
        }

        private void DebounceTimer_Tick(object? _, object __)
        {
            _debounceTimer.Stop();
            RegenerateFlyoutMenu();
        }

        private void Models_PropertyChanged(object? _, EventArgs __)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void RegenerateFlyoutMenu()
        {
            ModelFlyoutMenu.Items.Clear();

            if (_settings.Models.DefaultModel is not null)
            {
                var firstItem = new RadioMenuFlyoutItem
                {
                    Text = $"Por defecto ({_settings.Models.DefaultModel!.CardName})",
                    Tag = null,
                    Icon = new ImageIcon() { Source = _settings.Models.DefaultModel!.ModelProvider.GetIcon() },
                };

                firstItem.Click += ModelItem_Click;
                ModelFlyoutMenu.Items.Add(firstItem);
                ModelFlyoutMenu.Items.Add(new MenuFlyoutSeparator());

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

                        ModelFlyoutMenu.Items.Add(menuItem);

                        menuItem.Click += ModelItem_Click;

                        elementAdded = true;
                    }

                    if (elementAdded) ModelFlyoutMenu.Items.Add(new MenuFlyoutSeparator());
                }

                ModelFlyoutMenu.Items.RemoveAt(ModelFlyoutMenu.Items.Count - 1);

                Select(SelectedModel);
            }
        }

        private void ModelItem_Click(object sender, RoutedEventArgs __)
        {
            SelectedModel = (AIModelViewModel?)((RadioMenuFlyoutItem)sender).Tag;

            UpdateButtonContent();

            ((RadioMenuFlyoutItem)sender).IsChecked = true;
        }

        private void UpdateButtonContent()
        {
            if (SelectedModel is not null)
            {
                ModelName.Text = SelectedModel.CardName;
                ModelIcon.Source = SelectedModel.ModelProvider.GetIcon();
            }
            else if (_settings.Models.DefaultModel is not null)
            {
                ModelName.Text = ShowDefaultOnButtonContent
                    ? $"Por defecto ({_settings.Models.DefaultModel!.CardName})"
                    : _settings.Models.DefaultModel.CardName;
                ModelIcon.Source = _settings.Models.DefaultModel.ModelProvider.GetIcon();
            }
            else
            {
                ModelName.Text = "Unavailable";
                ModelIcon.Source = null;
            }
        }

        private void DefaultModel_Changed(object? _, EventArgs __)
        {
            if (_settings.Models.DefaultModel is not null && ModelFlyoutMenu.Items.Any())
            {
                var firstItem = (RadioMenuFlyoutItem)ModelFlyoutMenu.Items.First();

                firstItem.Text = $"Por defecto ({_settings.Models.DefaultModel!.CardName})";
                firstItem.Icon = new ImageIcon() { Source = _settings.Models.DefaultModel!.ModelProvider.GetIcon() };

                if (SelectedModel is null)
                {
                    ModelName.Text = _settings.Models.DefaultModel.CardName;
                    ModelIcon.Source = _settings.Models.DefaultModel.ModelProvider.GetIcon();

                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        await Task.Delay(100);
                        firstItem.IsChecked = true;
                    });
                }
            }
        }

        public void Dispose()
        {
            _settings.General.ProviderAvaibilityChanged -= Models_PropertyChanged;
            _settings.Models.ModelAvaibilityChanged -= Models_PropertyChanged;
            _settings.Models.DefaultModelChanged -= DefaultModel_Changed;

            GC.SuppressFinalize(this);
        }
    }
}
