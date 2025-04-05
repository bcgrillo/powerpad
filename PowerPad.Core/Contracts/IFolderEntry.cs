using PowerPad.Core.Models.FileSystem;

namespace PowerPad.Core.Contracts
{
    public interface IFolderEntry
    {
        string Name { get; }

        string Path { get; }

        EntryType Type { get; }

        DocumentStatus? Status { get; }

        Folder? Parent { get; }

        int? Position { get; }
    }
}