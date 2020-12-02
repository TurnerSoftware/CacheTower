using System.Threading.Tasks;

namespace CacheTower
{
	public interface ICacheLayer
	{
		ValueTask FlushAsync();
		ValueTask CleanupAsync();
		ValueTask EvictAsync(string cacheKey);
		ValueTask<CacheEntry<T>> GetAsync<T>(string cacheKey);
		ValueTask SetAsync<T>(string cacheKey, CacheEntry<T> cacheEntry);
		ValueTask<bool> IsAvailableAsync(string cacheKey);
	}
}
