using System;
using System.Threading.Tasks;

namespace CacheTower
{
	/// <remarks>
	/// A <see cref="CacheStack{TContext}"/> provides access to a <typeparamref name="TContext"/> in <see cref="GetOrSetAsync{T}(string, Func{T, TContext, Task{T}}, CacheSettings)"/>.
	/// This allows for the ability to inject dependencies during the cache refreshing process.
	/// </remarks>
	/// <typeparam name="TContext">The type of context that is passed to <see cref="GetOrSetAsync{T}(string, Func{T, TContext, Task{T}}, CacheSettings)"/>.</typeparam>
	/// <inheritdoc/>
	public class CacheStack<TContext> : CacheStack, ICacheStack<TContext>
	{
		private readonly ICacheContextActivator CacheContextActivator;

		/// <summary>
		/// Creates a new <see cref="CacheStack{TContext}"/> with the given <paramref name="contextFactory"/>, <paramref name="cacheLayers"/> and <paramref name="extensions"/>.
		/// </summary>
		/// <param name="contextFactory">The factory that provides the context. This is called for every cache item refresh.</param>
		/// <param name="cacheLayers">The cache layers to use for the current cache stack. The layers should be ordered from the highest priority to the lowest. At least one cache layer is required.</param>
		/// <param name="extensions">The cache extensions to use for the current cache stack.</param>
		public CacheStack(Func<TContext> contextFactory, ICacheLayer[] cacheLayers, ICacheExtension[] extensions) : base(cacheLayers, extensions)
		{
			CacheContextActivator =
				new FuncCacheContextActivator<TContext>(contextFactory ??
				                                        throw new ArgumentNullException(nameof(contextFactory)));
		}

		/// <summary>
		/// Creates a new <see cref="CacheStack{TContext}"/> with the given <paramref name="contextFactory"/>, <paramref name="cacheLayers"/> and <paramref name="extensions"/>.
		/// </summary>
		/// <param name="cacheContextActivator">The activator that provides the context. This is called for every cache item refresh.</param>
		/// <param name="cacheLayers">The cache layers to use for the current cache stack. The layers should be ordered from the highest priority to the lowest. At least one cache layer is required.</param>
		/// <param name="extensions">The cache extensions to use for the current cache stack.</param>
		public CacheStack(ICacheContextActivator cacheContextActivator, ICacheLayer[] cacheLayers, ICacheExtension[] extensions) : base(cacheLayers, extensions)
		{
			CacheContextActivator = cacheContextActivator ?? throw new ArgumentNullException(nameof(cacheContextActivator));
		}

		/// <inheritdoc/>
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
				using var scope = CacheContextActivator.BeginScope();
				var context = (TContext)scope.Resolve(typeof(TContext));
				return await getter(old, context);
			}, settings);
		}
	}
}
