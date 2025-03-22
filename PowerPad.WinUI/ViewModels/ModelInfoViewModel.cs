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
        private readonly AIModel _modelInfo;

        public string Name { get => _modelInfo.Name; }

        public ModelStatus ModelStatus { get => _modelInfo.Status; }

        public ModelProvider ModelProvider { get => _modelInfo.ModelProvider; }

        public long? Size { get => _modelInfo.Size; }

        public ModelInfoViewModel(AIModel modelInfo)
        {
            _modelInfo = modelInfo;
        }

        public string SizeToString()
        {
            const long kiloByte = 1024;
            const long megaByte = kiloByte * 1024;
            const long gigaByte = megaByte * 1024;
            const long teraByte = gigaByte * 1024;

            if (Size >= teraByte)
            {
                return $"{(double)Size / teraByte:F1}TB";
            }
            else if (Size >= gigaByte)
            {
                return $"{(double)Size / gigaByte:F1}GB";
            }
            else if (Size >= megaByte)
            {
                return $"{(double)Size / megaByte:F1}MB";
            }
            else if (Size >= kiloByte)
            {
                return $"{(double)Size / kiloByte:F1}KB";
            }
            else
            {
                return $"{Size} Bytes";
            }
        }
    }
}
