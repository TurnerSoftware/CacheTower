using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower.Providers.FileSystem
{
	public interface IManifestEntry
	{
		string FileName { get; set; }
		DateTime CachedAt { get; set; }
		TimeSpan TimeToLive { get; set; }
	}
}
