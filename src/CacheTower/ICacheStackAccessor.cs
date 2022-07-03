using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CacheTower;

/// <summary>
/// Provides access to a named implementation of <see cref="ICacheStack"/>.
/// </summary>
public interface ICacheStackAccessor
{
	/// <summary>
	/// Creates or returns existing named <see cref="ICacheStack"/> base on the configured builder.
	/// </summary>
	/// <param name="name">The name of the <see cref="ICacheStack"/> that has been configured.</param>
	/// <returns></returns>
	ICacheStack GetCacheStack(string name);
}

/// <summary>
/// Provides access to a named implementation of <see cref="ICacheStack{TContext}"/>.
/// </summary>
/// <typeparam name="TContext">The type of context that is passed during the cache entry generation process.</typeparam>
public interface ICacheStackAccessor<TContext>
{
	/// <summary>
	/// Creates or returns existing named <see cref="ICacheStack{TContext}"/> base on the configured builder.
	/// </summary>
	/// <param name="name">The name of the <see cref="ICacheStack{TContext}"/> that has been configured.</param>
	/// <returns></returns>
	ICacheStack<TContext> GetCacheStack(string name);
}


internal record NamedCacheStackProvider<TCacheStack>(string Name, Func<IServiceProvider, TCacheStack> Provider);
internal class NamedCacheStackLookup<TCacheStack>
	where TCacheStack : ICacheStack
{
	private readonly ConcurrentDictionary<string, Lazy<TCacheStack>> cachedDependencies = new(StringComparer.Ordinal);
	private readonly Dictionary<string, NamedCacheStackProvider<TCacheStack>> namedProviders;
	private readonly IServiceProvider serviceProvider;

	public NamedCacheStackLookup(
		IServiceProvider serviceProvider,
		IEnumerable<NamedCacheStackProvider<TCacheStack>> namedProviders
	)
	{
		this.serviceProvider = serviceProvider;
		this.namedProviders = namedProviders.ToDictionary(p => p.Name);
	}

	public TCacheStack GetCacheStack(string name)
	{
		if (!namedProviders.TryGetValue(name, out var dependencyProvider))
		{
			throw new InvalidOperationException($"No \"{typeof(TCacheStack)}\" is registered with the name \"{name}\"");
		}

		return cachedDependencies.GetOrAdd(name, name => new Lazy<TCacheStack>(() => dependencyProvider.Provider(serviceProvider))).Value;
	}
}

internal class CacheStackAccessor : ICacheStackAccessor
{
	private readonly NamedCacheStackLookup<ICacheStack> cacheStackAccessor;

	public CacheStackAccessor(NamedCacheStackLookup<ICacheStack> cacheStackAccessor)
	{
		this.cacheStackAccessor = cacheStackAccessor;
	}

	public ICacheStack GetCacheStack(string name) => cacheStackAccessor.GetCacheStack(name);
}

internal class CacheStackAccessor<TContext> : ICacheStackAccessor<TContext>
{
	private readonly NamedCacheStackLookup<ICacheStack<TContext>> cacheStackAccessor;

	public CacheStackAccessor(NamedCacheStackLookup<ICacheStack<TContext>> cacheStackAccessor)
	{
		this.cacheStackAccessor = cacheStackAccessor;
	}

	public ICacheStack<TContext> GetCacheStack(string name) => cacheStackAccessor.GetCacheStack(name);
}