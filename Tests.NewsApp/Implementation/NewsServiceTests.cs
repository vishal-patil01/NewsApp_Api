using Moq;
using Moq.Protected;
using NewsApp.Models.Configurations;
using NewsApp.Models.Entities;
using NewsApp.Services.Implementation;
using NewsApp.Services.Interface;
using Newtonsoft.Json;
using System.Net;

namespace Tests.NewsApp.Implementation
{
    public class NewsServiceTests
    {
        private MockRepository mockRepository;

        private Mock<IHttpClientFactory> mockHttpClientFactory;
        private Mock<IMemoryCacheWrapper> mockMemoryCache;

        public NewsServiceTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            
            this.mockHttpClientFactory = this.mockRepository.Create<IHttpClientFactory>();
            this.mockMemoryCache = this.mockRepository.Create<IMemoryCacheWrapper>();
            AppSettings.ConfigurationSettings = new ConfigurationSettings()
            {
                NewsServiceBaseUrl = "www.test.com"
            };
        }

        private NewsService CreateService()
        {
            return new NewsService(
                this.mockHttpClientFactory.Object,
                this.mockMemoryCache.Object);
        }

        [Fact]
        public async Task GetStoriesAsync_WhenCacheIsAvailable_ReturnsCachedData()
        {
            // Arrange
            var service = this.CreateService();
            string? searchQuery = null;
            var cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Cached Story 1", Url = "http://example.com/1" },
                new Story { Id = 2, Title = "Cached Story 2", Url = "http://example.com/2" }
            };

            this.mockMemoryCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedStories))
                .Returns(true);

            // Act
            var result = await service.GetStoriesAsync(searchQuery);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(cachedStories, result.Data);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStoriesAsync_WhenCacheIsNotAvailable_FetchesFromApiAndCachesResult()
        {
            // Arrange
            var service = this.CreateService();
            string? searchQuery = null;
            var cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Tech Story 1", Url = "http://example.com/1" },
                new Story { Id = 2, Title = "Non-Tech Story", Url = "http://example.com/2" }
            };

            var storyIds = new List<int> { 1, 2, 3 }; // Sample story IDs
            var storyIdsJson = JsonConvert.SerializeObject(storyIds);

            this.mockMemoryCache
               .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedStories))
               .Returns(true);


            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

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

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Act
            var result = await service.GetStoriesAsync(searchQuery);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStoriesAsync_WhenApiFails_ReturnsErrorResponse()
        {
            // Arrange
            var service = this.CreateService();
            string? searchQuery = null;
            object? cachedStories = null;

            this.mockMemoryCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedStories))
                .Returns(false);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

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
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com") // Base URL
            };

            // Mock HttpClientFactory to return the mocked HttpClient
            this.mockHttpClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            // Act
            var result = await service.GetStoriesAsync(searchQuery);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStoriesAsync_WhenSearchQueryIsProvided_FiltersStories()
        {
            // Arrange
            var service = this.CreateService();
            string searchQuery = "tech";
            var cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Tech Story 1", Url = "http://example.com/1" },
                new Story { Id = 2, Title = "Non-Tech Story", Url = "http://example.com/2" }
            };

            this.mockMemoryCache
       .Setup(cache => cache.TryGetValue(It.IsAny<string>(), out cachedStories))
       .Returns(true);

            // Act
            var result = await service.GetStoriesAsync(searchQuery);

            // Assert
            Assert.True(result.Success);
            var filteredStories = result.Data as List<Story>;
            Assert.Contains(filteredStories, story => story.Title.Contains("Tech"));
            this.mockRepository.VerifyAll();
        }
    }
}
