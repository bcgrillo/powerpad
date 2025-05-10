namespace PowerPad.Core.Models.FileSystem
{
    /// <summary>
    /// Represents the status of a document in the file system.
    /// </summary>
    public enum DocumentStatus
    {
        /// <summary>Document is not loaded.</summary>
        Unloaded,
        /// <summary>Document is not saved.</summary>
        Dirty,
        /// <summary>Document is saved automatically.</summary>
        AutoSaved,
        /// <summary>Document is saved.</summary>
        Saved
    }
}