namespace PowerPad.Core.Models.AI
{
    public record AIModel(string Name, ModelProvider ModelProvider, long? Size = null, string? DisplayName = null)
    {
        public string Name { get; init; } = Name;
        public ModelProvider ModelProvider { get; init; } = ModelProvider;
        public long? Size { get; set; } = Size;
        public string? DisplayName { get; set; } = DisplayName;
    }
}