using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Models;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using System.ClientModel;
using Uri = System.Uri;

namespace PowerPad.Core.Services.AI
{
    public interface IOpenAIService : IAIService
    {
    }

    public class OpenAIService : IOpenAIService
    {
        private OpenAIClient? _openAI;

        public void Initialize(AIServiceConfig config)
        {
            ArgumentException.ThrowIfNullOrEmpty(config.BaseUrl);
            ArgumentException.ThrowIfNullOrEmpty(config.Key);

            _openAI = new OpenAIClient(new ApiKeyCredential(config.Key), new OpenAIClientOptions { Endpoint = new Uri(config.BaseUrl) });
        }

        //public IEnumerable<AIModelInfo> GetModels()
        //{
        //    if (_models == null)
        //    {
        //        _models = _configStore.TryGet<Collection<AIModelInfo>>(StoreKey.OpenAIModels);

        //        if (_models == null)
        //        {
        //            _models = [.. StoreDefault.InitialOpenAIModels.Select(m => new AIModelInfo(new AIModel(m, ModelProvider.OpenAI), status: ModelStatus.Available))];
        //            _configStore.Set(StoreKey.OpenAIModels, _models);
        //        }
        //    }

        //    return _models;
        //}

        public async Task<IEnumerable<AIModel>> GetAvailableModels()
        {
            if (_openAI == null) return [];

            var models = await _openAI.GetOpenAIModelClient().GetModelsAsync();

            return models.Value.Select(m => CreateAIModel(m));
        }

        private static AIModel CreateAIModel(OpenAIModel openAIModel)
        {
            return new AIModel(openAIModel.Id, ModelProvider.OpenAI);
        }

        public IChatClient? ChatClient(AIModel model) => _openAI?.AsChatClient(model.Name);
    }
}
