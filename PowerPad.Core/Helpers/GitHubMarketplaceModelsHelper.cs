using PowerPad.Core.Models.AI;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace PowerPad.Core.Helpers
{
    /// <summary>
    /// Helper class for interacting with the GitHub Marketplace to search for AI models.
    /// Provides methods to query and filter models based on specific criteria.
    /// </summary>
    public static class GitHubMarketplaceModelsHelper
    {
        private const string GITHUB_BASE_URL = "https://github.com";
        private const string GITHUB_MARKETPLACE_SEARCH_URL = "https://github.com/marketplace?type=models&task=chat-completion&query=";
        private const string HEADER_ACCEPT = "Accept";
        private const string HEADER_APPLICATION_JSON = "application/json";
        private const string NAME_PREFIX = "/models/";

        // Restricted models found in https://github.githubassets.com/assets/ui/packages/github-models/utils/model-access.ts
        // These models fail with a free developer key (GITHUB_TOKEN).
        private static readonly string[] RESTRICTED_MODEL_NAMES = ["o", "o1-mini", "o1-preview", "o3-mini", "o3", "o4-mini", "o4"];

        private const int MAX_RESULTS = 20;

        /// <summary>
        /// Searches the GitHub Marketplace for AI models based on the provided query.
        /// Filters out restricted models and limits the results to a maximum number.
        /// </summary>
        /// <param name="query">The search query string to filter models.</param>
        /// <returns>A collection of AIModel objects matching the search criteria.</returns>
        public static async Task<IEnumerable<AIModel>> Search(string? query)
        {
            var url = GITHUB_MARKETPLACE_SEARCH_URL + Uri.EscapeDataString(query ?? string.Empty);
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(HEADER_ACCEPT, HEADER_APPLICATION_JSON);

            // Fetch search results from the GitHub Marketplace API.
            var searchResults = await httpClient.GetFromJsonAsync<GitHubMarketplaceResponse>(url);
            if (searchResults?.Results is null) return [];

            var results = new List<AIModel>();

            // Filter and process the search results.
            foreach (var model in searchResults.Results
                .Where(m => !RESTRICTED_MODEL_NAMES.Contains(m.Name))
                .Where(m => m.Name.Contains(query ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
                         || (m.Friendly_Name is not null && m.Friendly_Name.Contains(query ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)))
                .Take(MAX_RESULTS))
            {
                int startIndex = model.Id.IndexOf(NAME_PREFIX);
                if (startIndex != -1)
                {
                    startIndex += NAME_PREFIX.Length;
                    int endIndex = model.Id.IndexOf('/', startIndex);
                    if (endIndex != -1)
                    {
                        string modelName = model.Id[startIndex..endIndex];
                        results.Add(new AIModel(
                            $"{model.Publisher}/{modelName}",
                            ModelProvider.GitHub,
                            $"{GITHUB_BASE_URL}{model.Model_Url}"
                        ));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Represents the response structure from the GitHub Marketplace API.
        /// </summary>
        /// <param name="Results">The list of AI models returned from the search.</param>
        private sealed record GitHubMarketplaceResponse(List<GitHubModel> Results);

        /// <summary>
        /// Represents a single AI model entry in the GitHub Marketplace.
        /// </summary>
        /// <param name="Name">The unique name of the model.</param>
        /// <param name="Friendly_Name">An optional friendly name for the model.</param>
        /// <param name="Id">The unique identifier of the model.</param>
        /// <param name="Model_Url">The URL to access the model.</param>
        /// <param name="Publisher">The publisher of the model.</param>
        private sealed record GitHubModel(string Name, string? Friendly_Name, string Id, string Model_Url, string Publisher);
    }
}