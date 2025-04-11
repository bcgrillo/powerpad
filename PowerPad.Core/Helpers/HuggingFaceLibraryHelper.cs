using PowerPad.Core.Models.AI;
using System.Net.Http.Json;

namespace PowerPad.Core.Helpers
{
    public static class HuggingFaceLibraryHelper
    {
        private const string HUGGINGFACE_SEARCH_URL = "https://huggingface.co/api/models?filter=gguf&search=";
        private const string HUGGINGFACE_MODEL_URL = "https://huggingface.co/api/models/";
        private const int MAX_RESULTS = 5;

        public static async Task<IEnumerable<AIModel>> Search(string? query)
        {
            var url = HUGGINGFACE_SEARCH_URL + Uri.EscapeDataString(query ?? string.Empty);
            var httpClient = new HttpClient();

            // Realizar la búsqueda inicial
            var searchResults = await httpClient.GetFromJsonAsync<List<HuggingFaceModel>>(url);
            if (searchResults is null) return Enumerable.Empty<AIModel>();

            var results = new List<AIModel>();

            foreach (var model in searchResults.Take(MAX_RESULTS))
            {
                var modelDetailsUrl = HUGGINGFACE_MODEL_URL + model.id + "/tree/main";
                var modelFiles = await httpClient.GetFromJsonAsync<List<HuggingFaceFile>>(modelDetailsUrl);
                if (modelFiles is null) continue;

                var ggufFiles = modelFiles.Where(file => file.path.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase));
                foreach (var file in ggufFiles)
                {
                    results.Add(new AIModel(
                        $"hf.co/{model.id}:{file.path}",
                        ModelProvider.HuggingFace,
                        file.size,
                        $"{model.id}:{file.path}"
                    ));
                }
            }

            return results;
        }

        private record HuggingFaceModel(string id);
        private record HuggingFaceFile(string path, long size);
    }
}
