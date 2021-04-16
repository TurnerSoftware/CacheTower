using System.IO;

namespace CacheTower
{
    /// <summary>
    /// This abstraction allows us to use different encoding formats
    /// </summary>
    public interface ICacheSerializer
    {
        /// <summary>
        /// Serialize a cache entry onto a MemoryStream
        /// </summary>
        /// <param name="cacheEntry"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        MemoryStream Serialize<T>(T cacheEntry);

        /// <summary>
        /// Deserialize a cache entry from a MemoryStream
        /// </summary>
        /// <param name="stream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Deserialize<T>(MemoryStream stream);
    }
}