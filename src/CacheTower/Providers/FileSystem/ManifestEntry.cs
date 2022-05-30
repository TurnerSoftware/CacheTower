using System;

namespace CacheTower.Providers.FileSystem
{
	/// <summary>
	/// The manifest entry for a file system based cache.
	/// </summary>
	/// <param name="FileName">The file name that contains the cached data.</param>
	/// <param name="Expiry">The expiry date of the cached value.</param>
	public record struct ManifestEntry(string? FileName, DateTime Expiry)
	{
		/// <summary>
		/// The file name that contains the cached data.
		/// </summary>
		public string? FileName { get; init; } = FileName;
		/// <summary>
		/// The expiry date of the cached value.
		/// </summary>
		public DateTime Expiry { get; init; } = Expiry;
	}
}
