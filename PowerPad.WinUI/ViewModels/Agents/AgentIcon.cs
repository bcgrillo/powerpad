using Windows.UI;

namespace PowerPad.WinUI.ViewModels.Agents
{
    /// <summary>
    /// Represents the type of an agent icon.
    /// </summary>
    public enum AgentIconType
    {
        /// <summary>Base64-encoded image.</summary>
        Base64Image,
        /// <summary>Font glyph.</summary>
        FontIconGlyph
    }

    /// <summary>
    /// Represents an agent icon.
    /// </summary>
    /// <param name="Source">The source of the icon, which can be a Base64 string or a font glyph.</param>
    /// <param name="Type">The type of the icon, indicating whether it's a Base64 image or a font glyph.</param>
    /// <param name="Color">The color of the icon, if applicable.</param>
    public readonly record struct AgentIcon(string Source, AgentIconType Type, Color? Color = null);
}