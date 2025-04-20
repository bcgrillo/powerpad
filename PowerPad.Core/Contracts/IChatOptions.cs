using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.Core.Contracts
{
    internal interface IChatOptions
    {
        float? Temperature { get; set; }
        float? TopP { get; set; }
        int? MaxOutputTokens { get; set; }
    }
}