namespace PowerPad.Core.Models.FileSystem
{
    /// <summary>
    /// Represents the status of a document in the file system.
    /// </summary>
    public enum DocumentStatus
    {
        Unloaded,
        Dirty,
        AutoSaved,
        Saved
    }
}