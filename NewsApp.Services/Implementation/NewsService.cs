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
        private const string CacheKey = "TopStories";

        public NewsService(IHttpClientFactory httpClientFactory, IMemoryCacheWrapper memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            this._responseHelper = new ResponseHelper();
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
            var filteredStories = cachedStories
               .Where(story => !string.IsNullOrEmpty(story.Title) &&
                               story.Title.ToLower().Contains(searchQuery.ToLower()))
               .ToList();
            return filteredStories?.Count() <= 0 
                ? _responseHelper.HandleNotFound("Stories")
                : _responseHelper.HandleSuccess(filteredStories, "Stories fetched Successfully");
        }

        private async Task<List<Story>> FetchTopStoriesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{AppSettings.ConfigurationSettings.NewsServiceBaseUrl}/topstories.json");
            var response = await client.SendAsync(request);
            List<int> storyIds = new List<int>();
            var stories = new List<Story>();

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                storyIds= JsonConvert.DeserializeObject<List<int>>(content);
            }

            foreach (var id in storyIds)
            {
                var storyUrl = $"{AppSettings.ConfigurationSettings.NewsServiceBaseUrl}/item/{id}.json";
                var storyResponse = await client.GetStringAsync(storyUrl);
                var story = JsonConvert.DeserializeObject<Story>(storyResponse);

                if (story != null && !string.IsNullOrEmpty(story.Url))
                {
                    stories.Add(story);
                }

                if (stories.Count >= 200)
                {
                    break;
                }
            }

            return stories;
        }
    }
}

