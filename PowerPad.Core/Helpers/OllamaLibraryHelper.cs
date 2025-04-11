using HtmlAgilityPack;
using OllamaSharp;
using OllamaSharp.Models;
using PowerPad.Core.Models.AI;
using System.Threading.Tasks;

namespace PowerPad.Core.Helpers
{
    public static class OllamaLibraryHelper
    {
        private const string OLLAMA_BASE_URL = "https://ollama.com";
        private const string OLLAMA_SEARCH_URL = "https://ollama.com/search?q=";
        private const int MAX_RESULS = 5;

        public static async Task<IEnumerable<AIModel>> Search(string? query)
        {
            var url = OLLAMA_SEARCH_URL + Uri.EscapeDataString(query ?? string.Empty);
            var httpClient = new HttpClient();

            var response = await httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var modelLinks = htmlDoc.DocumentNode.SelectNodes("//a[starts-with(@href, '/library/')]").Take(MAX_RESULS);

            var results = new List<AIModel>();

            if (modelLinks is null) return [];

            foreach (var modelLink in modelLinks)
            {
                var href = modelLink.GetAttributeValue("href", "");
                var name = href.Split('/').LastOrDefault() ?? "";
                if (results.Any(m => m.Name == name)) continue;

                var responseAux = await httpClient.GetStringAsync(OLLAMA_BASE_URL + href);
                var auxDoc = new HtmlDocument();
                auxDoc.LoadHtml(responseAux);

                var tags = auxDoc.DocumentNode.SelectNodes("//a[contains(@class, 'group flex')]");
                if (tags is null) continue;

                foreach (var tag in tags)
                {
                    var tagName = tag.SelectSingleNode(".//span[@class='group-hover:underline']")?.InnerText.Trim() ?? "";
                    var tagSizeText = tag.SelectSingleNode(".//span[@class='text-xs text-neutral-400']")?.InnerText.Trim() ?? "";
                    var tagSize = ConvertSizeToBytes(tagSizeText);

                    results.Add(new AIModel(
                            $"{name}:{tagName}",
                            ModelProvider.Ollama,
                            tagSize
                        ));
                }
            }

            return results;
        }

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
