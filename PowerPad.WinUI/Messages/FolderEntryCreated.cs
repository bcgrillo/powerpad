using CommunityToolkit.Mvvm.Messaging.Messages;
using PowerPad.Core.Contracts;
using PowerPad.WinUI.ViewModels.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Messages
{
    public class FolderEntryCreated(FolderEntryViewModel value) : ValueChangedMessage<FolderEntryViewModel>(value)
    {
    }
}
