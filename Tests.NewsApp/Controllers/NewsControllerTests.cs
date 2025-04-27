using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsApp.API.Controllers;
using NewsApp.Models.Contracts;
using NewsApp.Services.Interface;
using System.Net;

namespace Tests.NewsApp.Controllers
{
    public class NewsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<INewsService> mockNewsService;
        private Dictionary<string, string> headers;

        public NewsControllerTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockNewsService = this.mockRepository.Create<INewsService>();
            headers = new Dictionary<string, string>
            {
                { "CorrelationId", "test-correlation-id" }
            };
        }
        public static HttpContext CreateHttpContext(Dictionary<string, string> headers = null)
        {
            var httpContext = new DefaultHttpContext();
            if (headers != null)
            {
                foreach (var dict in headers)
                {
                    httpContext.Request.Headers.Add(dict.Key, dict.Value);
                }
            }
            return httpContext;
        }


        private NewsController CreateNewsController()
        {
            var controller= new NewsController(this.mockNewsService.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = CreateHttpContext(headers)
            };
            return controller;
        }

        [Fact]
        public async Task GetStories_WhenCalledWithNullSearchTerm_ReturnsExpectedResult()
        {
            // Arrange
            var newsController = this.CreateNewsController();
            string? searchTerm = null;
            var expectedResponse = new BaseResponse
            {
                Success = true,
                Data = new { Stories = new[] { "Story1", "Story2" } },
                StatusCode = HttpStatusCode.OK
            };

            this.mockNewsService
                .Setup(service => service.GetStoriesAsync(searchTerm))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await newsController.GetStories(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStories_WhenServiceFails_ReturnsInternalServerError()
        {
            // Arrange
            var newsController = this.CreateNewsController();
            string? searchTerm = "test";
            var expectedResponse = new BaseResponse
            {
                Success = false,
                Message = "An error occurred",
                StatusCode = HttpStatusCode.InternalServerError
            };

            this.mockNewsService
                .Setup(service => service.GetStoriesAsync(searchTerm))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await newsController.GetStories(searchTerm);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
            Assert.Equal(expectedResponse, objectResult.Value);
            this.mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStories_WhenCalledWithValidSearchTerm_ReturnsExpectedStories()
        {
            // Arrange
            var newsController = this.CreateNewsController();
            string searchTerm = "technology";
            var expectedResponse = new BaseResponse
            {
                Success = true,
                Data = new { Stories = new[] { "Tech Story1", "Tech Story2" } },
                StatusCode = HttpStatusCode.OK
            };

            this.mockNewsService
                .Setup(service => service.GetStoriesAsync(searchTerm))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await newsController.GetStories(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);
            this.mockRepository.VerifyAll();
        }
    }
}
