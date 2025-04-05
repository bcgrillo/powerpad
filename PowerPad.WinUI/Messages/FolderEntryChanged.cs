using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.Core.Contracts;

namespace PowerPad.WinUI.Messages
{
    public class FolderEntryChanged(IFolderEntry value) : ValueChangedMessage<IFolderEntry>(value)
    {
        public bool NameChanged { get; set; }
    }
}