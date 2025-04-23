using PowerPad.Core.Models.AI;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace PowerPad.Core.Helpers
{
    public static class GitHubMarktplaceModelsHelper
    {
        private const string GITHUB_BASE_URL = "https://github.com";
        private const string GITHUB_MARKETPLACE_SEARCH_URL = "https://github.com/marketplace?type=models&task=chat-completion&query=";
        private const string HEADER_ACCEPT = "Accept";
        private const string HEADER_APPLICATION_JSON = "application/json";

        //Restricted models found in https://github.githubassets.com/assets/ui/packages/github-models/utils/model-access.ts
        //These models fails with free developer key (GITHUB_TOKEN)
        private static readonly string[] RESTRICTED_MODEL_NAMES = ["o", "o1-mini", "o1-preview", "o3-mini"];

        private const int MAX_RESULTS = 20;

        public static async Task<IEnumerable<AIModel>> Search(string? query)
        {
            var url = GITHUB_MARKETPLACE_SEARCH_URL + Uri.EscapeDataString(query ?? string.Empty);
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(HEADER_ACCEPT, HEADER_APPLICATION_JSON);

            var searchResults = await httpClient.GetFromJsonAsync<GitHubMarketplaceResponse>(url);
            if (searchResults?.Results is null) return [];

            var results = new List<AIModel>();

            foreach (var model in searchResults.Results
                .Where(m => !RESTRICTED_MODEL_NAMES.Contains(m.Name))
                .Where(m => m.Name.Contains(query ?? string.Empty, StringComparison.InvariantCultureIgnoreCase))
                .Take(MAX_RESULTS))
            {
                results.Add(new AIModel(
                    $"{model.Publisher}/{model.Name}",
                    ModelProvider.GitHub,
                    $"{GITHUB_BASE_URL}{model.Model_Url}"
                ));
            }

            return results;
        }

        private record GitHubMarketplaceResponse(List<GitHubModel> Results);

        private record GitHubModel(string Name, string Model_Url, string Publisher);
    }
}
