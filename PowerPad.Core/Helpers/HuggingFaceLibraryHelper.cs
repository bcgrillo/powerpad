using PowerPad.Core.Models.AI;
using System.Net.Http.Json;

namespace PowerPad.Core.Helpers
{
    /// <summary>
    /// Provides helper methods for interacting with the Hugging Face API, including searching for models
    /// and retrieving model details.
    /// </summary>
    public static class HuggingFaceLibraryHelper
    {
        private const string HF_OLLAMA_PREFIX = "hf.co";
        private const string HUGGINGFACE_BASE_URL = "https://huggingface.co/";
        private const string HUGGINGFACE_SEARCH_URL = "https://huggingface.co/api/models?filter=gguf,conversational&search=";
        private const string HUGGINGFACE_MODEL_URL = "https://huggingface.co/api/models/";
        private const int MAX_RESULTS = 20;

        /// <summary>
        /// Searches the Hugging Face API for models matching the specified query.
        /// </summary>
        /// <param name="query">The search query string. If null or empty, all models are returned.</param>
        /// <returns>A collection of AIModel objects representing the search results.</returns>
        public static async Task<IEnumerable<AIModel>> Search(string? query)
        {
            var url = HUGGINGFACE_SEARCH_URL + Uri.EscapeDataString(query ?? string.Empty);
            var httpClient = new HttpClient();

            var searchResults = await httpClient.GetFromJsonAsync<List<HuggingFaceModel>>(url);
            if (searchResults is null) return [];

            var results = new List<AIModel>();

            foreach (var modelId in searchResults.Select(m => m.Id).Take(MAX_RESULTS))
            {
                var modelDetailsUrl = $"{HUGGINGFACE_MODEL_URL}{modelId}/tree/main";
                var modelFiles = await httpClient.GetFromJsonAsync<List<HuggingFaceFile>>(modelDetailsUrl);
                if (modelFiles is null) continue;

                var ggufFiles = modelFiles.Where(file => file.Path.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase));
                foreach (var file in ggufFiles)
                {
                    var tag = ExtractTagFromFileName(file.Path);
                    results.Add(new AIModel(
                        $"{HF_OLLAMA_PREFIX}/{modelId}:{tag}",
                        ModelProvider.HuggingFace,
                        GetModelUrl(modelId),
                        file.Size,
                        $"{modelId}:{tag}"
                    ));
                }
            }

            return results;
        }

        /// <summary>
        /// Constructs the URL for a specific model on Hugging Face.
        /// </summary>
        /// <param name="modelName">The name of the model, optionally including a tag (e.g., "modelName:tag").</param>
        /// <returns>The URL of the model on Hugging Face.</returns>
        public static string GetModelUrl(string modelName)
        {
            var colonIndex = modelName.IndexOf(':');
            modelName = colonIndex >= 0 ? modelName[..colonIndex] : modelName;

            return $"{HUGGINGFACE_BASE_URL}{modelName}";
        }

        /// <summary>
        /// Extracts the tag from a file name, if present. The tag is assumed to be the portion of the file name
        /// after the last dash ('-').
        /// </summary>
        /// <param name="filePath">The full path of the file.</param>
        /// <returns>The extracted tag, or the file name if no tag is found.</returns>
        private static string ExtractTagFromFileName(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var lastDashIndex = fileName.LastIndexOf('-');
            return lastDashIndex >= 0 ? fileName[(lastDashIndex + 1)..] : fileName;
        }

        /// <summary>
        /// Represents a model retrieved from the Hugging Face API.
        /// </summary>
        private sealed record HuggingFaceModel(string Id);

        /// <summary>
        /// Represents a file associated with a model in the Hugging Face API.
        /// </summary>
        private sealed record HuggingFaceFile(string Path, long Size);
    }
}