using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Helpers
{
    public static class NameGeneratorAgent
    {
        private static readonly Agent _nameGeneratorAgent = new()
        { 
            Name = "NameGenerator",
            Prompt = "You are an expert in naming documents and conversations. Your task is to generate a name for the given content. " +
                     "You should consider the context and purpose of the content when generating the name. " +
                     "Please provide a concise and relevant name without any additional explanations or greetings. " +
                     "Important: the name should be able to be a file name, so avoid using special characters like /, \\, :, *, ?, \", <, >, |.",
            MaxOutputTokens = 100,
            Temperature = 0.1f,
            TopP = 1
        };

        public static async Task<string?> Generate(string fileContent, CancellationToken cancellationToken = default)
        {
            if (App.Get<SettingsViewModel>().IsAIAvailable == true)
            {
                var chatService = App.Get<IChatService>();
                var generateNameBuilder = new StringBuilder();

                await chatService.GetAgentResponse(fileContent, generateNameBuilder, _nameGeneratorAgent, null, null, cancellationToken);

                return generateNameBuilder.ToString().Trim();
            }

            return null;
        }
    }
}
