using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower.Providers.FileSystem
{
	public class ManifestEntry : IManifestEntry
	{
		public string FileName { get; set; }
		public DateTime Expiry { get; set; }
	}
}
