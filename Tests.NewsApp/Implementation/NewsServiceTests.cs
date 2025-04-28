using System.Net;
using Moq;
using Moq.Protected;
using NewsApp.Models.Configurations;
using NewsApp.Models.Entities;
using NewsApp.Services.Implementation;
using NewsApp.Services.Interface;
using Newtonsoft.Json;

namespace Tests.NewsApp.Implementation
{
    public class NewsServiceTests
    {
        private MockRepository mockRepository;

        private Mock<IHttpClientFactory> mockHttpClientFactory;
        private Mock<IMemoryCacheWrapper> mockMemoryCache;

        public NewsServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);

            mockHttpClientFactory = mockRepository.Create<IHttpClientFactory>();
            mockMemoryCache = mockRepository.Create<IMemoryCacheWrapper>();
            AppSettings.ConfigurationSettings = new ConfigurationSettings()
            {
                NewsServiceBaseUrl = "www.test.com"
            };
        }

        private NewsService CreateService()
        {
            return new NewsService(
                mockHttpClientFactory.Object,
                mockMemoryCache.Object);
        }

        [Fact]
        public async Task GetStoriesAsync_WhenCacheIsAvailable_ReturnsCachedData()
        {
            // Arrange
            NewsService service = CreateService();
            string? searchQuery = null;
            List<Story>? cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Cached Story 1", Url = "http://example.com/1" },
                new Story { Id = 2, Title = "Cached Story 2", Url = "http://example.com/2" }
            };

            mockMemoryCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedStories))
                .Returns(true);

            // Act
            global::NewsApp.Models.Contracts.BaseResponse result = await service.GetStoriesAsync(searchQuery);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(cachedStories, result.Data);
            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStoriesAsync_WhenCacheIsNotAvailable_FetchesFromApiAndCachesResult()
        {
            // Arrange
            NewsService service = CreateService();
            string? searchQuery = null;
            List<Story>? cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Tech Story 1", Url = "http://example.com/1" },
                new Story { Id = 2, Title = "Non-Tech Story", Url = "http://example.com/2" }
            };

            List<int> storyIds = new List<int> { 1, 2, 3 }; // Sample story IDs
            string storyIdsJson = JsonConvert.SerializeObject(storyIds);

            mockMemoryCache
               .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedStories))
               .Returns(true);


            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // Mock SendAsync for fetching story IDs
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get && req.RequestUri.ToString() == "https://hacker-news.firebaseio.com/v0/topstories.json"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(storyIdsJson)
                });

            // Mock GetStringAsync for fetching individual stories
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get && req.RequestUri.ToString() == "http://example.com/1"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(cachedStories[0]))
                });

            HttpClient httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Act
            global::NewsApp.Models.Contracts.BaseResponse result = await service.GetStoriesAsync(searchQuery);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStoriesAsync_WhenApiFails_ReturnsErrorResponse()
        {
            // Arrange
            NewsService service = CreateService();
            string? searchQuery = null;
            object? cachedStories = null;

            mockMemoryCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedStories))
                .Returns(false);

            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // Mock SendAsync to return an internal server error
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                });

            // Create the HttpClient using the mocked handler
            HttpClient httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com") // Base URL
            };

            // Mock HttpClientFactory to return the mocked HttpClient
            mockHttpClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            // Act
            var result = await service.GetStoriesAsync(searchQuery);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStoriesAsync_WhenSearchQueryIsProvided_FiltersStories()
        {
            // Arrange
            NewsService service = CreateService();
            string searchQuery = "tech";
            List<Story>? cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Tech Story 1", Url = "http://example.com/1" },
                new Story { Id = 2, Title = "Non-Tech Story", Url = "http://example.com/2" }
            };

            mockMemoryCache
       .Setup(cache => cache.TryGetValue(It.IsAny<string>(), out cachedStories))
       .Returns(true);

            // Act
            var result = await service.GetStoriesAsync(searchQuery);

            // Assert
            Assert.True(result.Success);
            List<Story>? filteredStories = result.Data as List<Story>;
            Assert.Contains(filteredStories, story => story.Title.Contains("Tech"));
            mockRepository.VerifyAll();
        }
    }
}
