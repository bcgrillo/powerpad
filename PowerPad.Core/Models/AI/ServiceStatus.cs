namespace PowerPad.Core.Models.AI
{
    /// <summary>
    /// Represents the status of an AI service.
    /// </summary>
    public enum ServiceStatus
    {
        Unknown,
        Unconfigured,
        Updating,
        Available,
        Online,
        Error,
        NotFound,
    }
}