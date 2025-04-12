namespace PowerPad.Core.Models.AI
{
    public record AIModel(string Name, ModelProvider ModelProvider, string? InfoUrl, long? Size = null, string? DisplayName = null)
    {
        public string Name { get; init; } = Name;
        public ModelProvider ModelProvider { get; init; } = ModelProvider;
        public string? InfoUrl { get; init; } = InfoUrl;
        public long? Size { get; set; } = Size;
        public string? DisplayName { get; set; } = DisplayName;
    }
}