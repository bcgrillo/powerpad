using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using PowerPad.WinUI.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels
{
    public partial class AIModelViewModel : ObservableObject
    {
        private readonly AIModel _aiModel;

        public AIModelViewModel(AIModel modelInfo)
        {
            _aiModel = modelInfo;
        }

        public string Name => _aiModel.Name;

        public bool Enabled
        {
            get => _aiModel.Enabled;
            set
            {
                SetProperty(_aiModel.Enabled, value, _aiModel, (x, y) => x.Enabled = y);
                WeakReferenceMessenger.Default.Send(new AIModelChanged(_aiModel));
            }
        }

        public long? Size
        {
            get => _aiModel.Size;
            set 
            {
                SetProperty(_aiModel.Size, value, _aiModel, (x, y) => x.Size = y);
                OnPropertyChanged(nameof(SizeAsString));
                WeakReferenceMessenger.Default.Send(new AIModelChanged(_aiModel));
            }
        }

        public string? DisplayName
        {
            get => _aiModel.DisplayName;
            set
            {
                SetProperty(_aiModel.DisplayName, value, _aiModel, (x, y) => x.DisplayName = y);
                WeakReferenceMessenger.Default.Send(new AIModelChanged(_aiModel));
            }
        }

        public string SizeAsString
        {
            get
            {
                if (Size == null) return string.Empty;

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
}