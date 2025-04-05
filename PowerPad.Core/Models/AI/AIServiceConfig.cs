namespace PowerPad.Core.Models.AI
{
    public record AIServiceConfig
    {
        public string? BaseUrl { get; set; } = null;
        public string? Key { get; set; } = null;
    }
}