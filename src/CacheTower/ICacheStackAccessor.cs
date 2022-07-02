using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CacheTower;

public interface ICacheStackAccessor
{
	ICacheStack GetCacheStack(string name);
}

public interface ICacheStackAccessor<TContext>
{
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