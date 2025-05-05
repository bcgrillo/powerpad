using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.AI;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Components.Controls
{
    public sealed partial class AgentSelector : UserControl, IDisposable
    {
        private readonly AgentsCollectionViewModel _agentsCollection;
        private bool _selectFirstAgent;

        public DocumentType DocumentType
        {
            get => (DocumentType)GetValue(DocumentTypeProperty);
            set
            {
                SetValue(DocumentTypeProperty, value);
                RegenerateFlyoutMenu();
            }
        }

        public static readonly DependencyProperty DocumentTypeProperty =
            DependencyProperty.Register(nameof(DocumentType), typeof(DocumentType), typeof(AgentSelector), new(DocumentType.Note));

        public AgentViewModel? SelectedAgent
        {
            get;
            private set
            {
                if (field != value)
                {
                    field = value;
                    SelectedAgentChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? SelectedAgentChanged;

        public AgentSelector()
        {
            this.InitializeComponent();

            _agentsCollection = App.Get<AgentsCollectionViewModel>();
        }

        public void Initialize(AgentViewModel? agent, bool selectFirstAgent = false)
        {
            _selectFirstAgent = selectFirstAgent;

            SelectedAgent = agent ?? (_selectFirstAgent ? _agentsCollection.Agents.FirstOrDefault() : null);

            RegenerateFlyoutMenu();

            _agentsCollection.AgentsAvaibilityChanged += Agents_AgentsAvaibilityChanged;
        }

        public void ShowMenu()
        {
            AgentButton.Flyout?.ShowAt(AgentButton);
        }

        private void Select(AgentViewModel? agent)
        {
            if (agent is null)
            {
                SelectedAgent = null;
            }
            else
            {
                var menuItem = (RadioMenuFlyoutItem?)AgentFlyoutMenu.Items
                    .FirstOrDefault(i => i.Tag as AgentViewModel == agent);

                if (menuItem is not null)
                {
                    SelectedAgent = agent;
                }
                else
                {
                    SelectedAgent = null;
                }
            }

            DispatcherQueue.TryEnqueue(() => UpdateChekedItemMenu());
            UpdateButtonContent();
        }

        private async void UpdateChekedItemMenu()
        {
            await Task.Delay(100);

            foreach (RadioMenuFlyoutItem item in AgentFlyoutMenu.Items.OfType<RadioMenuFlyoutItem>())
                item.IsChecked = false;

            var menuItem = SelectedAgent is null
                ? null
                : (RadioMenuFlyoutItem?)AgentFlyoutMenu.Items.FirstOrDefault(i => i.Tag as AgentViewModel == SelectedAgent);

            if (menuItem is not null) menuItem.IsChecked = true;
        }

        private void Agents_AgentsAvaibilityChanged(object? _, EventArgs __)
        {
            SelectedAgent ??= (_selectFirstAgent ? _agentsCollection.Agents.FirstOrDefault() : null);

            RegenerateFlyoutMenu();
        }

        private void RegenerateFlyoutMenu()
        {
            AgentFlyoutMenu.Items.Clear();

            var enabledAgents = GetEnabledAgents();

            if (enabledAgents.Any())
            {
                foreach (var agent in enabledAgents)
                {
                    var menuItem = new RadioMenuFlyoutItem
                    {
                        Text = agent.Name,
                        Tag = agent,
                        Icon = agent.IconElement
                    };

                    AgentFlyoutMenu.Items.Add(menuItem);

                    menuItem.Click += AgentItem_Click;
                }

                Select(SelectedAgent);
            }
            else
            {
                Select(null);
            }
        }

        private IEnumerable<AgentViewModel> GetEnabledAgents()
        {
            return DocumentType switch
            {
                DocumentType.Note => _agentsCollection.Agents.Where(a => a.ShowInNotes),
                DocumentType.Chat => _agentsCollection.Agents.Where(a => a.ShowInChats),
                _ => throw new NotImplementedException()
            };
        }

        private void AgentItem_Click(object sender, RoutedEventArgs __)
        {
            SelectedAgent = (AgentViewModel?)((RadioMenuFlyoutItem)sender).Tag;

            ((RadioMenuFlyoutItem)sender).IsChecked = true;

            UpdateButtonContent();
        }

        private void UpdateButtonContent()
        {
            if (SelectedAgent is not null)
            {
                AgentIconControl.AgentIcon = SelectedAgent.Icon;
                AgentName.Text = SelectedAgent.Name;
            }
            else
            {
                AgentIconControl.AgentIcon = new("\uE99A", AgentIconType.FontIconGlyph);
                AgentName.Text = "Seleccione agente";
            }
        }

        public void Dispose()
        {
            _agentsCollection.AgentsAvaibilityChanged -= Agents_AgentsAvaibilityChanged;

            GC.SuppressFinalize(this);
        }
    }
}
