using System.Buffers;
using System.IO;

namespace CacheTower
{
    /// <summary>
    /// This abstraction allows us to use different encoding formats
    /// </summary>
    public interface ICacheSerializer
    {
		/// <summary>
		/// Serializes a <paramref name="value"/> to the specified <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="value"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		void Serialize<T>(Stream stream, T value);

        /// <summary>
        /// Deserializes <typeparamref name="T"/> from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Deserialize<T>(Stream stream);
    }
}