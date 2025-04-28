
using Microsoft.AspNetCore.Mvc;
using NewsApp.Services.Helpers;
using NewsApp.Services.Interface;

namespace NewsApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsServic;
        private readonly ResponseHelper _responseHelper;

        public NewsController(INewsService newsService)
        {
            _newsServic = newsService;
            _responseHelper = new ResponseHelper();
        }

        /// <summary>
        /// Retrieves a list of news stories optionally filtered by a search term.
        /// </summary>
        /// <param name="searchTerm">
        /// An optional query parameter to filter news stories based on the search criteria.
        /// </param>
        /// <returns>A standardized HTTP response with the list of news stories.</returns>
        /// <remarks>
        /// Calls the INewsService.GetStoriesAsync method which returns a <see cref="BaseResponse"/>.
        /// The <see cref="ResponseHelper.HandleResponse"/> method is then used to generate an ActionResult,
        /// handling successful and error scenarios uniformly.
        /// </remarks>
        [HttpGet("stories")]
        public async Task<IActionResult> GetStories([FromQuery] string? searchTerm = null)
        {
            var data = await _newsServic.GetStoriesAsync(searchTerm);
            return _responseHelper.HandleResponse(this, data);
        }
    }
}
