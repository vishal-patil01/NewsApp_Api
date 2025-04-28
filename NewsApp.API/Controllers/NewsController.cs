
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

        [HttpGet("stories")]
        public async Task<IActionResult> GetStories([FromQuery] string? searchTerm = null)
        {
            Models.Contracts.BaseResponse data = await _newsServic.GetStoriesAsync(searchTerm);
            return _responseHelper.HandleResponse(this, data);
        }
    }
}
