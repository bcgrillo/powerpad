using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI
{
    /// <summary>
    /// Represents the menu options available for managing AI models in the application.
    /// </summary>
    public enum MenuOption
    {
        /// <summary>Option to view available AI models.</summary>
        AvailableModels,
        /// <summary>Option to add new AI models.</summary>
        AddModels
    }

    /// <summary>
    /// Represents a menu option associated with a specific AI model provider.
    /// </summary>
    /// <param name="ModelProvider">The AI model provider associated with the menu option.</param>
    /// <param name="Option">The selected menu option.</param>
    public readonly record struct ModelsMenuOption(ModelProvider ModelProvider, MenuOption Option)
    {
        /// <summary>
        /// Creates a new instance of <see cref="ModelsMenuOption"/>.
        /// </summary>
        /// <param name="ModelProvider">The AI model provider associated with the menu option.</param>
        /// <param name="Option">The selected menu option.</param>
        /// <returns>A new instance of <see cref="ModelsMenuOption"/>.</returns>
        public static ModelsMenuOption Create(ModelProvider ModelProvider, MenuOption Option) => new(ModelProvider, Option);
    }
}