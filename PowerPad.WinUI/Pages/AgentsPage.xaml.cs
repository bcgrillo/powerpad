using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.Components.Editors;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Settings;
using Windows.UI;

namespace PowerPad.WinUI.Pages
{
    public partial class AgentsPage : DisposablePage, IToggleMenuPage
    {
        private readonly AgentsCollectionViewModel _agentsCollection;
        private readonly SettingsViewModel _settings; 
        
        private AgentViewModel? _selectedAgent;
        private AgentEditorControl? _editorControl;

        public double NavigationWidth => AgentsMenu.Visibility == Visibility.Visible ? TreeView.ActualWidth : 0;

        public AgentsPage()
        {
            this.InitializeComponent();

            _agentsCollection = App.Get<AgentsCollectionViewModel>();
            _settings = App.Get<SettingsViewModel>();
        }

        public event EventHandler? NavigationVisibilityChanged;

        private async void TreeView_ItemInvoked(TreeView _, TreeViewItemInvokedEventArgs eventArgs)
        {
            var invokedEntry = (AgentViewModel)eventArgs.InvokedItem;
            bool cancel = false;

            if (_editorControl is not null)
            {
                var result = await _editorControl.ConfirmClose();

                if (result)
                {
                    AgentEditorContent.Children.Clear();
                    _editorControl.Dispose();
                    _editorControl = null;
                    _selectedAgent = null;
                }
                else cancel = true;
            }
            else UpdateLandingVisibility(showLanding: false);

            if (cancel)
            {
                TreeView.SelectedItem = _selectedAgent;
            }
            else
            {
                _selectedAgent = invokedEntry;
                _editorControl = new AgentEditorControl(invokedEntry, XamlRoot);
                AgentEditorContent.Children.Add(_editorControl);
            }
        }

        private void UpdateLandingVisibility(bool showLanding)
        {
            if (showLanding)
            {
                AgentLanding.Visibility = Visibility.Visible;
                AgentEditorContent.Visibility = Visibility.Collapsed;
            }
            else
            {
                AgentLanding.Visibility = Visibility.Collapsed;
                AgentEditorContent.Visibility = Visibility.Visible;
            }
        }

        public void ToggleNavigationVisibility()
        {
            AgentsMenu.Visibility = AgentsMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            NavigationVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void NewAgentButton_Click(object _, RoutedEventArgs __)
        {
            var newIcon = GenerateNewIcon();
            var newAgent = new AgentViewModel(new Agent { Name = "Nuevo agente", Prompt = "Eres un agente que..."}, newIcon);

            _agentsCollection.Agents.Add(newAgent);

            DispatcherQueue.TryEnqueue(() =>
            {
                TreeView.SelectedItem = newAgent;
            });
        }

        private AgentIcon GenerateNewIcon()
        {
            var mode = _settings.General.AppTheme ?? Application.Current.RequestedTheme;
            var random = new Random();
            Color color;

            if (mode == ApplicationTheme.Dark)
            {
                color = Color.FromArgb(255,
                    (byte)random.Next(150, 200),
                    (byte)random.Next(150, 200),
                    (byte)random.Next(150, 200));
            }
            else
            {
                color = Color.FromArgb(255,
                    (byte)random.Next(50, 100),
                    (byte)random.Next(50, 100),
                    (byte)random.Next(50, 100));
            }
            
            return new ("\uE99A", AgentIconType.FontIconGlyph, color);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}