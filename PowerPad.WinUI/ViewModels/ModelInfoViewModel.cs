using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using PowerPad.Core.Configuration;
using PowerPad.Core.Models;
using PowerPad.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PowerPad.WinUI.Configuration.ConfigConstants;

namespace PowerPad.WinUI.ViewModels
{
    public partial class ModelInfoViewModel : ObservableObject
    {
        private readonly AIModel _modelInfo;
        private string? _displayName;

        public string Name { get => _displayName ?? _modelInfo.Name; }

        public ModelStatus ModelStatus { get => _modelInfo.Status; }

        public ModelProvider ModelProvider { get => _modelInfo.ModelProvider; }

        public long? Size { get => _modelInfo.Size; }

        public readonly RelayCommand SetDefaultModelCommand;

        public ModelInfoViewModel(AIModel modelInfo, string? displayName = null)
        {
            _modelInfo = modelInfo;
            _displayName = displayName;

            SetDefaultModelCommand = new RelayCommand(SetAsDefault);
        }

        public string SizeToString()
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

        private void SetAsDefault()
        {
            var aiService = Ioc.Default.GetRequiredService<IAIService>();
            var configStore = Ioc.Default.GetRequiredService<IConfigStore>();

            configStore.Set(Keys.DefaultModel, _modelInfo);
            aiService.SetDefaultModel(_modelInfo);
        }
    }
}
