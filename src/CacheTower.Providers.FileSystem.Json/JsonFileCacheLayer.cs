using System.IO;
using System.Text;
using Newtonsoft.Json;
using CacheTower.Serializers.NewtonsoftJson;
using System;

namespace CacheTower.Providers.FileSystem.Json
{
	/// <remarks>
	/// The <see cref="JsonFileCacheLayer"/> uses <a href="https://github.com/JamesNK/Newtonsoft.Json/">Newtonsoft.Json</a> to serialize and deserialize the cache items to the file system.
	/// </remarks>
	/// <inheritdoc/>
	[Obsolete("Use FileCacheLayer and specify the NewtonsoftJsonCacheSerializer. This cache layer (and the associated package) will be discontinued in a future release.")]
	public class JsonFileCacheLayer : FileCacheLayer, ICacheLayer
	{
		/// <summary>
		/// Creates a <see cref="JsonFileCacheLayer"/>, using the given <paramref name="directoryPath"/> as the location to store the cache.
		/// </summary>
		/// <param name="directoryPath"></param>
		public JsonFileCacheLayer(string directoryPath) : base(new NewtonsoftJsonCacheSerializer(), directoryPath, ".json") { }
	}
}
