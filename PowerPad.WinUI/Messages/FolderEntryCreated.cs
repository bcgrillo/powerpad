using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.WinUI.ViewModels.FileSystem;

namespace PowerPad.WinUI.Messages
{
    public class FolderEntryCreated(FolderEntryViewModel value) : ValueChangedMessage<FolderEntryViewModel>(value)
    {
    }
}
