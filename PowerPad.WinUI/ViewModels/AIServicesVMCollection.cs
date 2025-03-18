using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class AIServicesVMCollection : ObservableObject
    {
        [ObservableProperty]
        public required ObservableCollection<AIServiceViewModel> _services;

        public AIServicesVMCollection()
        {
            Services = [];
        }

        public T? GetVM<T>() where T : AIServiceViewModel
        {
            return Services.FirstOrDefault(s => s is T) as T;
        }
    }
}
