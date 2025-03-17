using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Messages
{
    public class FolderEntryDeleted : ValueChangedMessage<IFolderEntry>
    {
        public FolderEntryDeleted(IFolderEntry value) : base(value)
        {
        }
    }
}
