using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower.Tests
{
	public abstract class TestBase
	{
		protected static Task DisposeOf(IDisposable disposable)
		{
			disposable.Dispose();
			return Task.CompletedTask;
		}


#if !NET461
		protected static async Task DisposeOf(IAsyncDisposable disposable)
		{
			await disposable.DisposeAsync();
		}
#endif
	}
}
