using Windows.UI;

namespace PowerPad.WinUI.ViewModels.Agents
{
    public enum AgentIconType
    {
        Base64Image,
        FontIconGlyph
    }

    public readonly record struct AgentIcon(string Source, AgentIconType Type, Color? Color = null);
}