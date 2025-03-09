using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.Core.Models
{
    public interface IFolderEntry
    {
        string Name { get; }

        string Path { get; }

        EntryType Type { get; }

        DocumentStatus Status { get; }

        IFolderEntry? Parent { get; }
    }
}
