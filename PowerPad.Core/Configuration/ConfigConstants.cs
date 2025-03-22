using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.Core.Configuration
{
    public static class ConfigConstants
    {
        public class Keys
        {
            public const string GitHubModels = "GitHubModels";
            public const string OpenAIModels = "OpenAIModels";
        }

        public class Defaults
        {
            public static string[] InitialGutHubModels =
                ["gpt-4o", "gpt-4o-mini", "DeepSeek-R1", "DeepSeek-V3", "Llama-3-3-70B-Instruct", "Phi-4"];

            public static string[] InitialOpenAIModels =
                ["gpt-4o", "gpt-4o-mini", "o3-mini", "o1-mini", "o1"];
        }
    }
}
