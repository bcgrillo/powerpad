using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.WinUI.ViewModels.FileSystem;

namespace PowerPad.WinUI.Messages
{
    /// <summary>
    /// Message indicating that a folder entry has been created.
    /// </summary>
    /// <param name="value">The folder entry that was created.</param>
    public class FolderEntryCreated(FolderEntryViewModel value) : ValueChangedMessage<FolderEntryViewModel>(value)
    {
    }
}