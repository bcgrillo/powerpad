using HtmlAgilityPack;
using PowerPad.Core.Models.AI;

namespace PowerPad.Core.Helpers
{
    /// <summary>
    /// Provides helper methods for interacting with the Ollama library, including searching for AI models
    /// and generating model URLs.
    /// </summary>
    public static class OllamaLibraryHelper
    {
        private const string OLLAMA_BASE_URL = "https://ollama.com";
        private const string OLLAMA_LIBRARY_URL = "https://ollama.com/library";
        private const string OLLAMA_SEARCH_URL = "https://ollama.com/search?q=";
        private const int MAX_RESULTS = 20;

        /// <summary>
        /// Searches the Ollama library for AI models based on the provided query.
        /// </summary>
        /// <param name="query">The search query string. If null or empty, all models are returned.</param>
        /// <returns>A collection of AIModel objects representing the search results.</returns>
        public static async Task<IEnumerable<AIModel>> Search(string? query)
        {
            var url = OLLAMA_SEARCH_URL + Uri.EscapeDataString(query ?? string.Empty);
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var modelNodes = htmlDoc.DocumentNode?.SelectNodes("//li[@x-test-model]")?.Take(MAX_RESULTS);

            var results = new List<AIModel>();

            if (modelNodes is null) return results;

            foreach (var modelNode in modelNodes)
            {
                // Skip nodes containing <span x-test-capability ...>embedding</span>
                var capabilityNode = modelNode.SelectSingleNode(".//span[@x-test-capability and text()='embedding']");
                if (capabilityNode != null) continue;

                var linkNode = modelNode.SelectSingleNode(".//a[1]");
                if (linkNode is null) continue;

                var href = linkNode.GetAttributeValue("href", "");
                var name = href.Trim('/').Replace("library/", string.Empty);
                if (results.Any(m => m.Name == name)) continue;

                var responseAux = await httpClient.GetStringAsync($"{OLLAMA_BASE_URL}{href}");
                var auxDoc = new HtmlDocument();
                auxDoc.LoadHtml(responseAux);

                var tags = auxDoc.DocumentNode.SelectNodes("//div[contains(@class, 'hidden group')]");
                if (tags is null) continue;

                foreach (var tag in tags)
                {
                    var tagName = tag.SelectSingleNode(".//a[contains(@class, 'group-hover:underline')]")?.InnerText.Trim() ?? "";
                    var tagSizeText = tag.SelectSingleNode(".//p[@class='col-span-2 text-neutral-500']")?.InnerText.Trim() ?? "";
                    var tagSize = ConvertSizeToBytes(tagSizeText);

                    results.Add(new AIModel(
                            tagName,
                            ModelProvider.Ollama,
                            GetModelUrl(name),
                            tagSize
                        ));
                }
            }

            return results;
        }

        /// <summary>
        /// Generates a URL for the specified AI model name.
        /// </summary>
        /// <param name="modelName">The name of the AI model.</param>
        /// <returns>The URL of the AI model in the Ollama library.</returns>
        public static string GetModelUrl(string modelName)
        {
            var colonIndex = modelName.IndexOf(':');
            modelName = colonIndex >= 0 ? modelName[..colonIndex] : modelName;

            if (modelName.Contains('/')) return $"{OLLAMA_BASE_URL}/{modelName}";
            return $"{OLLAMA_LIBRARY_URL}/{modelName}";
        }

        /// <summary>
        /// Converts a size string (e.g., "10MB", "2GB") into its equivalent size in bytes.
        /// </summary>
        /// <param name="size">The size string to convert.</param>
        /// <returns>The size in bytes as a long value. Returns 0 if the input is invalid.</returns>
        private static long ConvertSizeToBytes(string size)
        {
            if (string.IsNullOrWhiteSpace(size)) return 0;

            var sizeUnit = size[^2..].ToUpperInvariant();
            var sizeValue = double.Parse(size[..^2], System.Globalization.CultureInfo.InvariantCulture);

            return sizeUnit switch
            {
                "KB" => (long)(sizeValue * 1024),
                "MB" => (long)(sizeValue * 1024 * 1024),
                "GB" => (long)(sizeValue * 1024 * 1024 * 1024),
                "TB" => (long)(sizeValue * 1024 * 1024 * 1024 * 1024),
                _ => 0
            };
        }
    }
}