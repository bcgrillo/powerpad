using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class AIServiceViewModel : ObservableObject
    {
        [ObservableProperty]
        public string _name;

        [ObservableProperty]
        public string? _glyph;

        [ObservableProperty]
        public ModelProvider _provider;

        [ObservableProperty]
        public ObservableCollection<ModelInfoViewModel>? _models;

        public AIServiceViewModel(string name, string glyph, ModelProvider provider)
        {
            Name = name;
            Glyph = glyph;
            Provider = provider;
        }
    }
}
