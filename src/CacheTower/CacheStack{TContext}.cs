using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CacheTower;

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
	/// Creates a new <see cref="CacheStack{TContext}"/> with the provided <paramref name="cacheContextActivator"/> and <paramref name="options"/>.
	/// </summary>
	/// <param name="logger">The internal logger to use.</param>
	/// <param name="cacheContextActivator">The activator that provides the context. This is called for every cache item refresh.</param>
	/// <param name="options">The <see cref="CacheStackOptions"/> to configure this cache stack.</param>
	public CacheStack(ILogger<CacheStack>? logger, ICacheContextActivator cacheContextActivator, CacheStackOptions options) : base(logger, options)
	{
		CacheContextActivator = cacheContextActivator ?? throw new ArgumentNullException(nameof(cacheContextActivator));
	}

	/// <summary>
	/// Creates a new <see cref="CacheStack{TContext}"/> with the given <paramref name="cacheContextActivator"/>, <paramref name="cacheLayers"/> and <paramref name="extensions"/>.
	/// </summary>
	/// <param name="logger">The internal logger to use.</param>
	/// <param name="cacheContextActivator">The activator that provides the context. This is called for every cache item refresh.</param>
	/// <param name="cacheLayers">The cache layers to use for the current cache stack. The layers should be ordered from the highest priority to the lowest. At least one cache layer is required.</param>
	/// <param name="extensions">The cache extensions to use for the current cache stack.</param>
	[Obsolete("Use constructor with 'CacheStackOptions'")]
	public CacheStack(ILogger<CacheStack>? logger, ICacheContextActivator cacheContextActivator, ICacheLayer[] cacheLayers, ICacheExtension[] extensions) : base(logger, cacheLayers, extensions)
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
			return await getter(old, context).ConfigureAwait(false);
		}, settings).ConfigureAwait(false);
	}
}
