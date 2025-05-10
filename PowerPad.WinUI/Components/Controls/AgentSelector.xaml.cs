using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.WinUI.ViewModels.Agents;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Components.Controls
{
    /// <summary>
    /// Represents a control for selecting an agent based on the document type.
    /// </summary>
    public sealed partial class AgentSelector : UserControl, IDisposable
    {
        private readonly AgentsCollectionViewModel _agentsCollection;
        private bool _selectFirstAgent;

        /// <summary>
        /// Gets or sets the document type to filter agents.
        /// </summary>
        public DocumentType DocumentType
        {
            get => (DocumentType)GetValue(DocumentTypeProperty);
            set
            {
                SetValue(DocumentTypeProperty, value);
                RegenerateFlyoutMenu();
            }
        }

        /// <summary>
        /// Dependency property for <see cref="DocumentType"/>.
        /// </summary>
        public static readonly DependencyProperty DocumentTypeProperty =
            DependencyProperty.Register(nameof(DocumentType), typeof(DocumentType), typeof(AgentSelector), new(DocumentType.Note));

        /// <summary>
        /// Gets the currently selected agent.
        /// </summary>
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

        /// <summary>
        /// Event triggered when the selected agent changes.
        /// </summary>
        public event EventHandler? SelectedAgentChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentSelector"/> class.
        /// </summary>
        public AgentSelector()
        {
            this.InitializeComponent();

            _agentsCollection = App.Get<AgentsCollectionViewModel>();
        }

        /// <summary>
        /// Initializes the control with a specific agent and optionally selects the first available agent.
        /// </summary>
        /// <param name="agent">The agent to initialize with.</param>
        /// <param name="selectFirstAgent">Whether to select the first available agent if none is provided.</param>
        public void Initialize(AgentViewModel? agent, bool selectFirstAgent = false)
        {
            _selectFirstAgent = selectFirstAgent;

            SelectedAgent = agent ?? (_selectFirstAgent ? _agentsCollection.Agents.FirstOrDefault() : null);

            RegenerateFlyoutMenu();

            _agentsCollection.AgentsAvailabilityChanged += Agents_AgentsAvailabilityChanged;
        }

        /// <summary>
        /// Displays the flyout menu for agent selection.
        /// </summary>
        public void ShowMenu()
        {
            AgentButton.Flyout?.ShowAt(AgentButton);
        }

        /// <summary>
        /// Selects the specified agent and updates the UI accordingly.
        /// </summary>
        /// <param name="agent">The agent to select.</param>
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

            DispatcherQueue.TryEnqueue(UpdateCheckedItemMenu);
            UpdateButtonContent();
        }

        /// <summary>
        /// Updates the checked state of menu items in the flyout menu.
        /// </summary>
        private async void UpdateCheckedItemMenu()
        {
            await Task.Delay(100);

            foreach (RadioMenuFlyoutItem item in AgentFlyoutMenu.Items.OfType<RadioMenuFlyoutItem>())
                item.IsChecked = false;

            var menuItem = SelectedAgent is null
                ? null
                : (RadioMenuFlyoutItem?)AgentFlyoutMenu.Items.FirstOrDefault(i => i.Tag as AgentViewModel == SelectedAgent);

            menuItem?.IsChecked = true;
        }

        /// <summary>
        /// Handles changes in agent availability and updates the menu.
        /// </summary>
        private void Agents_AgentsAvailabilityChanged(object? _, EventArgs __)
        {
            SelectedAgent ??= (_selectFirstAgent ? _agentsCollection.Agents.FirstOrDefault() : null);

            RegenerateFlyoutMenu();
        }

        /// <summary>
        /// Regenerates the flyout menu based on the enabled agents.
        /// </summary>
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

        /// <summary>
        /// Retrieves the list of enabled agents based on the document type.
        /// </summary>
        /// <returns>A collection of enabled agents.</returns>
        private IEnumerable<AgentViewModel> GetEnabledAgents()
        {
            return DocumentType switch
            {
                DocumentType.Note => _agentsCollection.Agents.Where(a => a.ShowInNotes),
                DocumentType.Chat => _agentsCollection.Agents.Where(a => a.ShowInChats),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Handles the click event for an agent menu item.
        /// </summary>
        /// <param name="sender">The menu item that was clicked.</param>
        private void AgentItem_Click(object sender, RoutedEventArgs __)
        {
            SelectedAgent = (AgentViewModel?)((RadioMenuFlyoutItem)sender).Tag;

            ((RadioMenuFlyoutItem)sender).IsChecked = true;

            UpdateButtonContent();
        }

        /// <summary>
        /// Updates the content of the button to reflect the selected agent.
        /// </summary>
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

        /// <inheritdoc />
        public void Dispose()
        {
            _agentsCollection.AgentsAvailabilityChanged -= Agents_AgentsAvailabilityChanged;

            GC.SuppressFinalize(this);
        }
    }
}