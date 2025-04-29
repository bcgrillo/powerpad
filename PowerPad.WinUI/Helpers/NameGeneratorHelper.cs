using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.FileSystem;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Helpers
{
    public static class NameGeneratorHelper
    {
        private const string NEW_FOLDER_NAME = "Nueva carpeta";
        private const int GENERATE_NAME_TIMEOUT = 10000;
        private const int MAX_NAME_LENGHT = 100;

        private static readonly Dictionary<DocumentType, string> NEW_DOCUMENT_NAMES = new()
        {
            { DocumentType.Note, "Nueva nota" },
            { DocumentType.Chat, "Nuevo chat" }
        };

        private static readonly Agent NAME_GENERATOR_AGENT = new()
        {
            Name = "NameGenerator",
            Prompt = "Your task is to generate a title for the given content. " +
                     "Please provide ONLY a short concise and relevant title " +
                     "for a computer document without any explanations or greetings. " +
                     "Try to keep the title under 50 characters.",
            MaxOutputTokens = 100,
            Temperature = 0.1f,
            TopP = 1
        };

        public static async Task<string?> Generate(string fileContent)
        {
            if (App.Get<SettingsViewModel>().IsAIAvailable == true)
            {
                var chatService = App.Get<IChatService>();
                var generateNameBuilder = new StringBuilder();

                using var cts = new CancellationTokenSource(GENERATE_NAME_TIMEOUT);

                try
                {
                    await chatService.GetAgentSingleResponse(fileContent, generateNameBuilder, NAME_GENERATOR_AGENT, null, null, cts.Token);

                    var generatedName = generateNameBuilder.ToString().Trim();

                    // Clean the generated name to remove invalid characters and extensions  
                    var invalidChars = Path.GetInvalidFileNameChars();
                    var cleanedName = new string([.. generatedName.Where(c => !invalidChars.Contains(c))]);
                    cleanedName = Path.GetFileNameWithoutExtension(cleanedName).Replace('_', ' ');
                    cleanedName = cleanedName[..Math.Min(cleanedName.Length, MAX_NAME_LENGHT)];

                    return cleanedName;
                }
                catch
                {
                    //TODO: Trace
                }
            }

            return null;
        }

        public static string NewFolderName() => NEW_FOLDER_NAME;

        public static string NewDocumentName(DocumentType type) => NEW_DOCUMENT_NAMES[type];

        public static bool CheckNewNamePattern(string name)
        {
            var patterns = NEW_DOCUMENT_NAMES.Select(x => x.Value);

            foreach (var pattern in patterns)
            {
                if (name == pattern ||
                    (name.StartsWith(pattern + " (") &&
                     name.EndsWith(')') &&
                     int.TryParse(name.AsSpan(pattern.Length + 2, name.Length - pattern.Length - 3), out _)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
