using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Configuration
{
    public record AzureAIConfig
    (
        string BaseUrl,
        string Key
    );
}
