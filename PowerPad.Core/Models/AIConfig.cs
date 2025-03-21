using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.Core.Models
{
    public record AIConfig
    (
        string? SystemPrompt = null,
        float? Temperature = null,
        int? MaxOutputTokens = null
    );
}