using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Models;
using PowerPad.Core.Configuration;
using PowerPad.Core.Models;
using System.ClientModel;
using System.Collections.ObjectModel;
using static PowerPad.Core.Configuration.ConfigConstants;
using Uri = System.Uri;

namespace PowerPad.Core.Services
{
    public interface IOpenAIService
    {
        IEnumerable<AIModel> GetModels();
        Task<IEnumerable<AIModel>> GetAvaliableModels();
        IChatClient ChatClient(AIModel model);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly OpenAIClient? _openAI;
        private IConfigStore _configStore;
        private Collection<AIModel>? _models;

        public OpenAIService(string baseUrl, string key, IConfigStore configStore)
        {
            _openAI = new OpenAIClient(new ApiKeyCredential(key), 
                new OpenAIClientOptions { Endpoint = new Uri(baseUrl) });

            _configStore = configStore;
        }

        public IEnumerable<AIModel> GetModels()
        {
            if (_models == null)
            {
                _models = _configStore.TryGet<Collection<AIModel>>(Keys.OpenAIModels);

                if (_models == null)
                {
                    _models = [.. Defaults.InitialOpenAIModels.Select(m => new AIModel(m, ModelProvider.OpenAI, Status: ModelStatus.Available))];
                    _configStore.Set(Keys.OpenAIModels, _models);
                }
            }

            return _models;
        }

        public async Task<IEnumerable<AIModel>> GetAvaliableModels()
        {
            if (_openAI == null) return [];

            var models = await _openAI.GetOpenAIModelClient().GetModelsAsync();

            return models.Value.Select(m => CreateAIModel(m));
        }

        private static AIModel CreateAIModel(OpenAIModel openAIModel)
        {
            return new AIModel(
                openAIModel.Id,
                ModelProvider.OpenAI,
                Status: ModelStatus.Available
            );
        }

        public IChatClient ChatClient(AIModel model)
        {
            if (_openAI == null) throw new InvalidOperationException("Open AI service not initialized.");

            return _openAI.AsChatClient(model.Name);
        }
    }
}
