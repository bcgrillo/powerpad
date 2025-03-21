using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Configuration
{
    public static class ConfigConstants
    {
        public class Keys
        {
            public const string RecentlyWorkspaces = "RecentlyWorkspaces";
            public const string OllamaServiceUrl = "OllamaServiceUrl";
            public const string DefaultModel = "DefaultModel";
        }

        public class Defaults
        {
            public static readonly string WorkspaceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nameof(PowerPad));
            public const string OllamaServiceUrl = "http://localhost:11434";
            public static readonly AIModel DefaultModel = new AIModel("gemma3:latest", ModelProvider.Ollama);
        }
    }
}
