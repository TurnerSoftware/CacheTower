using System;
using System.Threading.Tasks;

namespace CacheTower
{
	public class CacheStack<TContext> : CacheStack, ICacheStack<TContext>
	{
		private Func<TContext> ContextFactory { get; }

		public CacheStack(Func<TContext> contextFactory, ICacheLayer[] cacheLayers, ICacheExtension[] extensions) : base(cacheLayers, extensions)
		{
			ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
		}

		public async ValueTask<T> GetOrSetAsync<T>(string cacheKey, Func<T, TContext, Task<T>> getter, CacheSettings settings)
		{
			ThrowIfDisposed();

			if (cacheKey == null)
			{
				throw new ArgumentNullException(nameof(cacheKey));
			}

			if (getter == null)
			{
				throw new ArgumentNullException(nameof(getter));
			}

			return await GetOrSetAsync<T>(cacheKey, async (old) =>
			{
				var context = ContextFactory();
				return await getter(old, context);
			}, settings);
		}
	}
}
