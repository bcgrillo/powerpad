namespace PowerPad.WinUI.ViewModels.AI
{
    public enum AgentIconType
    {
        Base64Image,
        CharacterOrEmoji,
        FontIconGlyph
    }

    public readonly record struct AgentIcon(string IconSource, AgentIconType IconType);
}