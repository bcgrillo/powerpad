using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.Core.Contracts;

namespace PowerPad.WinUI.Messages
{
    /// <summary>  
    /// Message indicating that a folder entry has been changed.  
    /// </summary>  
    /// <param name="value">The folder entry that was changed.</param>  
    public class FolderEntryChanged(IFolderEntry value) : ValueChangedMessage<IFolderEntry>(value)
    {
        /// <summary>  
        /// Gets or sets a value indicating whether the name of the folder entry has changed.  
        /// </summary>  
        public bool NameChanged { get; set; }
    }
}