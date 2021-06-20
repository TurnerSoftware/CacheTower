using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower.Providers.FileSystem
{
	/// <summary>
	/// The manifest entry for a file system based cache.
	/// </summary>
	public class ManifestEntry
	{
		/// <summary>
		/// The file name that contains the cached data.
		/// </summary>
		public string? FileName { get; set; }
		/// <summary>
		/// The expiry date of the cached value.
		/// </summary>
		public DateTime Expiry { get; set; }
	}
}
