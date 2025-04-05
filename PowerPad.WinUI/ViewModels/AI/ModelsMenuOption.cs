using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI
{
    public enum MenuOption
    {
        AvailableModels,
        AddModels
    }

    public readonly record struct ModelsMenuOption(ModelProvider ModelProvider, MenuOption Option)
    {
        public static ModelsMenuOption Create(ModelProvider ModelProvider, MenuOption Option) => new(ModelProvider, Option);
    }
}