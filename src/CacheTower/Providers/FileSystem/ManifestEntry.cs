using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower.Providers.FileSystem
{
	/// <inheritdoc/>
	public class ManifestEntry : IManifestEntry
	{
		/// <inheritdoc/>
		public string? FileName { get; set; }
		/// <inheritdoc/>
		public DateTime Expiry { get; set; }
	}
}
