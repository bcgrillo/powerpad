using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class WorkspaceViewModel : ObservableObject
    {
        private readonly IWorkspaceService _workspaceService;

        [ObservableProperty]
        private FolderEntryViewModel _root;

        public WorkspaceViewModel(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;

            Root = new FolderEntryViewModel(_workspaceService.OpenWorkspace(@"D:\OneDrive\Escritorio\Universidad\PruebasTFG"));
        }
    }
}
