using PowerPad.Core.Models.AI;
using System.Net.Http.Json;

namespace PowerPad.Core.Helpers
{
    public static class HuggingFaceLibraryHelper
    {
        private const string HF_OLLAMA_PREFIX = "hf.co";
        private const string HUGGINGFACE_BASE_URL = "https://huggingface.co/";
        private const string HUGGINGFACE_SEARCH_URL = "https://huggingface.co/api/models?filter=gguf,conversational&search=";
        private const string HUGGINGFACE_MODEL_URL = "https://huggingface.co/api/models/";
        private const int MAX_RESULTS = 20;

        public static async Task<IEnumerable<AIModel>> Search(string? query)
        {
            var url = HUGGINGFACE_SEARCH_URL + Uri.EscapeDataString(query ?? string.Empty);
            var httpClient = new HttpClient();

            var searchResults = await httpClient.GetFromJsonAsync<List<HuggingFaceModel>>(url);
            if (searchResults is null) return [];

            var results = new List<AIModel>();

            foreach (var model in searchResults.Take(MAX_RESULTS))
            {
                var modelDetailsUrl = $"{HUGGINGFACE_MODEL_URL}{model.Id}/tree/main";
                var modelFiles = await httpClient.GetFromJsonAsync<List<HuggingFaceFile>>(modelDetailsUrl);
                if (modelFiles is null) continue;

                var ggufFiles = modelFiles.Where(file => file.Path.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase));
                foreach (var file in ggufFiles)
                {
                    var tag = ExtractTagFromFileName(file.Path);
                    results.Add(new AIModel(
                        $"{HF_OLLAMA_PREFIX}/{model.Id}:{tag}",
                        ModelProvider.HuggingFace,
                        GetModelUrl(model.Id),
                        file.Size,
                        $"{model.Id}:{tag}"
                    ));
                }
            }

            return results;
        }

        public static string GetModelUrl(string modelName)
        {
            var colonIndex = modelName.IndexOf(':');
            modelName = colonIndex >= 0 ? modelName[..colonIndex] : modelName;

            return $"{HUGGINGFACE_BASE_URL}{modelName}";
        }

        private static string ExtractTagFromFileName(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var lastDashIndex = fileName.LastIndexOf('-');
            return lastDashIndex >= 0 ? fileName[(lastDashIndex + 1)..] : fileName;
        }

        private record HuggingFaceModel(string Id);
        private record HuggingFaceFile(string Path, long Size);
    }
}
