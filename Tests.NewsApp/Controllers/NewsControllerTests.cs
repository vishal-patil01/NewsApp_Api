using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsApp.API.Controllers;
using NewsApp.Models.Contracts;
using NewsApp.Services.Interface;

namespace Tests.NewsApp.Controllers
{
    public class NewsControllerTests
    {
        private MockRepository mockRepository;

        private Mock<INewsService> mockNewsService;
        private Dictionary<string, string> headers;

        public NewsControllerTests()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);
            mockNewsService = mockRepository.Create<INewsService>();
            headers = new Dictionary<string, string>
            {
                { "CorrelationId", "test-correlation-id" }
            };
        }
        public static HttpContext CreateHttpContext(Dictionary<string, string> headers = null)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> dict in headers)
                {
                    httpContext.Request.Headers.Add(dict.Key, dict.Value);
                }
            }
            return httpContext;
        }


        private NewsController CreateNewsController()
        {
            NewsController controller = new NewsController(mockNewsService.Object);
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
            NewsController newsController = CreateNewsController();
            string? searchTerm = null;
            Response expectedResponse = new Response
            {
                Success = true,
                Data = new { Stories = new[] { "Story1", "Story2" } },
                StatusCode = HttpStatusCode.OK
            };

            mockNewsService
                .Setup(service => service.GetStoriesAsync(searchTerm))
                .ReturnsAsync(expectedResponse);

            // Act
            IActionResult result = await newsController.GetStories(searchTerm);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);
            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStories_WhenServiceFails_ReturnsInternalServerError()
        {
            // Arrange
            NewsController newsController = CreateNewsController();
            string? searchTerm = "test";
            Response expectedResponse = new Response
            {
                Success = false,
                Message = "An error occurred",
                StatusCode = HttpStatusCode.InternalServerError
            };

            mockNewsService
                .Setup(service => service.GetStoriesAsync(searchTerm))
                .ReturnsAsync(expectedResponse);

            // Act
            IActionResult result = await newsController.GetStories(searchTerm);

            // Assert
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
            Assert.Equal(expectedResponse, objectResult.Value);
            mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetStories_WhenCalledWithValidSearchTerm_ReturnsExpectedStories()
        {
            // Arrange
            NewsController newsController = CreateNewsController();
            string searchTerm = "technology";
            Response expectedResponse = new Response
            {
                Success = true,
                Data = new { Stories = new[] { "Tech Story1", "Tech Story2" } },
                StatusCode = HttpStatusCode.OK
            };

            mockNewsService
                .Setup(service => service.GetStoriesAsync(searchTerm))
                .ReturnsAsync(expectedResponse);

            // Act
            IActionResult result = await newsController.GetStories(searchTerm);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);
            mockRepository.VerifyAll();
        }
    }
}
