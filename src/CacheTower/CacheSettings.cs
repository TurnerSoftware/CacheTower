using System;
using System.Collections.Generic;
using System.Text;
using CacheTower.Providers.Memory;

namespace CacheTower
{
	public struct CacheSettings
	{
		/// <summary>
		/// The number of cache hits before forward propagating from a <see cref="MemoryCacheLayer" /> to higher level caches.
		/// </summary>
		public uint ForwardPropagateAfterXCacheHits { get; set; }
	}
}
