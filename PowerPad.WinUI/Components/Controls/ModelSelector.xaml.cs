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
    /// <summary>
    /// Represents a control for selecting AI models with a flyout menu.
    /// </summary>
    public sealed partial class ModelSelector : UserControl, IDisposable
    {
        private const double DEBOURCE_INTERVAL = 200;

        private readonly SettingsViewModel _settings;
        private readonly DispatcherTimer _debounceTimer;

        /// <summary>
        /// Gets or sets a value indicating whether to show the default model on the button content.
        /// </summary>
        public bool ShowDefaultOnButtonContent
        {
            get => (bool)GetValue(ShowDefaultOnButtonContentProperty);
            set
            {
                SetValue(ShowDefaultOnButtonContentProperty, value);
                RegenerateFlyoutMenu();
            }
        }

        /// <summary>
        /// Dependency property for <see cref="ShowDefaultOnButtonContent"/>.
        /// </summary>
        public static readonly DependencyProperty ShowDefaultOnButtonContentProperty =
            DependencyProperty.Register(nameof(ShowDefaultOnButtonContent), typeof(bool), typeof(AgentSelector), new(false));

        /// <summary>
        /// Gets or sets the currently selected AI model.
        /// </summary>
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

        /// <summary>
        /// Event triggered when the selected model changes.
        /// </summary>
        public event EventHandler? SelectedModelChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSelector"/> class.
        /// </summary>
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

        /// <summary>
        /// Initializes the control with the specified AI model.
        /// </summary>
        /// <param name="model">The AI model to initialize with.</param>
        public void Initialize(AIModelViewModel? model)
        {
            SelectedModel = model;
            RegenerateFlyoutMenu();

            _settings.General.ProviderAvailabilityChanged += Models_PropertyChanged;
            _settings.Models.ModelAvailabilityChanged += Models_PropertyChanged;
            _settings.Models.DefaultModelChanged += DefaultModel_Changed;
        }

        /// <summary>
        /// Updates the layout of the control based on the enabled state.
        /// </summary>
        /// <param name="newValue">The new enabled state.</param>
        public void UpdateEnabledLayout(bool newValue)
        {
            ModelIcon.UpdateEnabledLayout(newValue);
            UpdateCheckedItemMenu();
        }

        /// <summary>
        /// Selects the specified AI model.
        /// </summary>
        /// <param name="model">The AI model to select.</param>
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

            UpdateCheckedItemMenu();
            UpdateButtonContent();
        }

        /// <summary>
        /// Updates the checked state of items in the flyout menu.
        /// </summary>
        private async void UpdateCheckedItemMenu()
        {
            await Task.Delay(100);

            foreach (RadioMenuFlyoutItem item in ModelFlyoutMenu.Items.OfType<RadioMenuFlyoutItem>())
                item.IsChecked = false;

            var menuItem = SelectedModel is null
                ? (RadioMenuFlyoutItem?)ModelFlyoutMenu.Items.FirstOrDefault()
                : (RadioMenuFlyoutItem?)ModelFlyoutMenu.Items.FirstOrDefault(i => i.Tag as AIModelViewModel == SelectedModel);

            menuItem?.IsChecked = true;
        }

        /// <summary>
        /// Handles the debounce timer tick event to regenerate the flyout menu.
        /// </summary>
        private void DebounceTimer_Tick(object? _, object __)
        {
            _debounceTimer.Stop();
            RegenerateFlyoutMenu();
        }

        /// <summary>
        /// Handles property changes in the models or providers.
        /// </summary>
        private void Models_PropertyChanged(object? _, EventArgs __)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        /// <summary>
        /// Regenerates the flyout menu with the available models and providers.
        /// </summary>
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

        /// <summary>
        /// Handles the click event for a model item in the flyout menu.
        /// </summary>
        private void ModelItem_Click(object sender, RoutedEventArgs __)
        {
            SelectedModel = (AIModelViewModel?)((RadioMenuFlyoutItem)sender).Tag;

            UpdateButtonContent();

            ((RadioMenuFlyoutItem)sender).IsChecked = true;
        }

        /// <summary>
        /// Updates the content of the button based on the selected model.
        /// </summary>
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

        /// <summary>
        /// Handles changes to the default model.
        /// </summary>
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

        /// <inheritdoc />
        public void Dispose()
        {
            _settings.General.ProviderAvailabilityChanged -= Models_PropertyChanged;
            _settings.Models.ModelAvailabilityChanged -= Models_PropertyChanged;
            _settings.Models.DefaultModelChanged -= DefaultModel_Changed;

            GC.SuppressFinalize(this);
        }
    }
}