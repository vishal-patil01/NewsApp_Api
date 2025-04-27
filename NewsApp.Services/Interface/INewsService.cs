using NewsApp.Models.Contracts;

namespace NewsApp.Services.Interface
{
    public interface INewsService
    {
        Task<BaseResponse> GetStoriesAsync(string? searchQuery = null);
    }
}
