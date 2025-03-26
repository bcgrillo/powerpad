using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using OllamaSharp.Models;
using PowerPad.Core.Models;
using static PowerPad.Core.Configuration.ConfigConstants;
using System.Collections.ObjectModel;
using Uri = System.Uri;
using PowerPad.Core.Configuration;

namespace PowerPad.Core.Services
{
    public interface IAzureAIService
    {
        IEnumerable<AIModel> GetModels();
        Task<IEnumerable<AIModel>> GetAvaliableModels();
        IChatClient ChatClient(AIModel model);
    }

    public class AzureAIService : IAzureAIService
    {
        private readonly ChatCompletionsClient _azureAI;
        private IConfigStore _configStore;
        private Collection<AIModel>? _models;

        public AzureAIService(string baseUrl, string key, IConfigStore configStore)
        {
            _azureAI = new ChatCompletionsClient(new Uri(baseUrl), new AzureKeyCredential(key));

            _configStore = configStore;
        }

        public IEnumerable<AIModel> GetModels()
        {
            if (_models == null)
            {
                _models = _configStore.TryGet<Collection<AIModel>>(Keys.GitHubModels);

                if (_models == null)
                {
                    _models = [.. Defaults.InitialGutHubModels.Select(m => new AIModel(m, ModelProvider.GitHub, Status: ModelStatus.Available))];
                    _configStore.Set(Keys.GitHubModels, _models);
                }
            }

            return _models;
        }

        public async Task<IEnumerable<AIModel>> GetAvaliableModels()
        {
            return await Task.FromResult<IEnumerable<AIModel>>([]);
        }

        public IChatClient ChatClient(AIModel model)
        {
            if (_azureAI == null) throw new InvalidOperationException("Azure AI service not initialized.");

            return _azureAI.AsChatClient(model.Name);
        }
    }
}
