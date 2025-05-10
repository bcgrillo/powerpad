namespace PowerPad.Core.Models.AI
{
    /// <summary>
    /// Represents the status of an AI service.
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>Status is unknown</summary>
        Unknown,
        /// <summary>Service is not configured</summary>
        Unconfigured,
        /// <summary>Status is being updated</summary>
        Updating,
        /// <summary>Service is available</summary>
        Available,
        /// <summary>Service is online</summary>
        Online,
        /// <summary>Service with error</summary>
        Error,
        /// <summary>Service not found</summary>
        NotFound,
    }
}