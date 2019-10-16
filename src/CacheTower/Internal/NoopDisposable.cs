using System;
using System.Collections.Generic;
using System.Text;

namespace CacheTower.Internal
{
	internal struct NoopDisposable : IDisposable
	{
		public void Dispose()
		{
		}
	}
}
