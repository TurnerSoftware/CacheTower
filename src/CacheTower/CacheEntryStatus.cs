using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower
{
	/// <summary>
	/// Describes the status of the entry - whether it is expired, stale or access to an entry was a miss.
	/// </summary>
	internal enum CacheEntryStatus
	{
		/// <summary>
		/// When the cache entry is considered stale.
		/// </summary>
		Stale,
		/// <summary>
		/// When the cache entry is considered expired.
		/// </summary>
		Expired,
		/// <summary>
		/// When no cache entry was found.
		/// </summary>
		Miss
	}
}
