using CommunityToolkit.Mvvm.ComponentModel;
using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.ViewModels
{
    public partial class ModelInfoViewModel : ObservableObject
    {
        private ModelInfo _modelInfo;

        public string Name { get => _modelInfo.Name; }

        public ModelStatus ModelStatus { get => _modelInfo.Status; }

        public ModelInfoViewModel(ModelInfo modelInfo)
        {
            _modelInfo = modelInfo;
        }
    }
}
