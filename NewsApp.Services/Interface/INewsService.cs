using NewsApp.Models.Contracts;

namespace NewsApp.Services.Interface
{
    public interface INewsService
    {
        Task<Response> GetStoriesAsync(string? searchQuery = null);
    }
}
