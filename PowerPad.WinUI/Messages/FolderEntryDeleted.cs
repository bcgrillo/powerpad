using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.Core.Contracts;

namespace PowerPad.WinUI.Messages
{
    public class FolderEntryDeleted(IFolderEntry value) : ValueChangedMessage<IFolderEntry>(value)
    {
    }
}
