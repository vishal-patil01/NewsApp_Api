namespace NewsApp.Services.Interface
{
    public interface IMemoryCacheWrapper
    {
        void Set<T>(object key, T value, TimeSpan expiration);
        bool TryGetValue<T>(object key, out T value);
    }
}