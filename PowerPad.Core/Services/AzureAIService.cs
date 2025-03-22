using ABI.System;
using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using OllamaSharp;
using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exception = System.Exception;
using Uri = System.Uri;

namespace PowerPad.Core.Services
{
    public interface IAzureAIService
    {
        Task<IEnumerable<AIModel>> GetModels();
        IChatClient ChatClient(AIModel model);
    }

    public class AzureAIService : IAzureAIService
    {
        private readonly ChatCompletionsClient? _azureAI;

        public AzureAIService(string baseUrl, string key)
        {
            _azureAI = new ChatCompletionsClient(new Uri(baseUrl), new AzureKeyCredential(key));
        }

        public async Task<IEnumerable<AIModel>> GetModels()
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
