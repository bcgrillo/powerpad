namespace PowerPad.Core.Contracts
{
    internal interface IChatOptions
    {
        float? Temperature { get; set; }
        float? TopP { get; set; }
        int? MaxOutputTokens { get; set; }
    }
}