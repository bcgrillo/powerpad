using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Configuration
{
    public class GeneralSettings
    {
        public required bool OllamaEnabled { get; set; }
        public required bool AzureAIEnabled { get; set; }
        public required bool OpenAIEnabled { get; set; }
    };
}
