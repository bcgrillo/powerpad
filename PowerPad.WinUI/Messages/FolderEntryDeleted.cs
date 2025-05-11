using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.Core.Contracts;

namespace PowerPad.WinUI.Messages
{
    /// <summary>
    /// Message indicating that a folder entry has been deleted.
    /// </summary>
    /// <param name="value">The folder entry that was deleted.</param>
    public class FolderEntryDeleted(IFolderEntry value) : ValueChangedMessage<IFolderEntry>(value)
    {
    }
}