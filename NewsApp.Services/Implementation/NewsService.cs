using NewsApp.Models.Configurations;
using NewsApp.Models.Contracts;
using NewsApp.Models.Entities;
using NewsApp.Services.Helpers;
using NewsApp.Services.Interface;
using Newtonsoft.Json;

namespace NewsApp.Services.Implementation
{
    public class NewsService : INewsService
    {
        private readonly ResponseHelper _responseHelper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCacheWrapper _memoryCache;
        private static string CacheKey = $"TopStories_{AppSettings.ConfigurationSettings.MaxStoriesCount}";

        public NewsService(IHttpClientFactory httpClientFactory, IMemoryCacheWrapper memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _responseHelper = new ResponseHelper();
        }

        public async Task<BaseResponse> GetStoriesAsync(string? searchQuery = null)
        {
            // Check if cached data exists
            if (!_memoryCache.TryGetValue(CacheKey, out List<Story> cachedStories))
            {
                cachedStories = await FetchTopStoriesAsync();
                if (cachedStories?.Count() <= 0)
                    return _responseHelper.HandleNotFound("Stories");
                _memoryCache.Set(CacheKey, cachedStories, TimeSpan.FromHours(8));
            }

            if (string.IsNullOrEmpty(searchQuery))
            {
                return _responseHelper.HandleSuccess(cachedStories, "Stories fetched Successfully");
            }
            List<Story>? filteredStories = cachedStories
               .Where(story => !string.IsNullOrEmpty(story.Title) &&
                               story.Title.ToLower().Contains(searchQuery.ToLower()))
               .ToList();
            return filteredStories?.Count() <= 0
                ? _responseHelper.HandleNotFound("Stories")
                : _responseHelper.HandleSuccess(filteredStories, "Stories fetched Successfully");
        }

        private async Task<List<Story>> FetchTopStoriesAsync()
        {
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{AppSettings.ConfigurationSettings.NewsServiceBaseUrl}/topstories.json");
            HttpResponseMessage response = await client.SendAsync(request);
            List<int> storyIds = new List<int>();
            List<Story> stories = new List<Story>();

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                storyIds = JsonConvert.DeserializeObject<List<int>>(content);
            }

            foreach (int id in storyIds)
            {
                string storyUrl = $"{AppSettings.ConfigurationSettings.NewsServiceBaseUrl}/item/{id}.json";
                string storyResponse = await client.GetStringAsync(storyUrl);
                Story? story = JsonConvert.DeserializeObject<Story>(storyResponse);

                if (story != null && !string.IsNullOrEmpty(story.Url))
                {
                    stories.Add(story);
                }

                if (stories.Count >= AppSettings.ConfigurationSettings.MaxStoriesCount)
                {
                    break;
                }
            }

            return stories;
        }
    }
}

