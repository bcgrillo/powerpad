using PowerPad.Core.Models.AI;

namespace PowerPad.WinUI.ViewModels.AI
{
    public enum MenuOption
    {
        AvailableModels,
        AddModels
    }

    public struct ModelsMenuOption
    {
        public ModelProvider ModelProvider { get; set; }
        public MenuOption Option { get; set; }

        public static ModelsMenuOption Create(ModelProvider modelProvider, MenuOption option)
        {
            return new ModelsMenuOption
            {
                ModelProvider = modelProvider,
                Option = option
            };
        }
    }
}