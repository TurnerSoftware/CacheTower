using System;
using System.Collections.Generic;
using System.Linq;
using CacheTower;
using CacheTower.Extensions;
using CacheTower.Providers.FileSystem;
using CacheTower.Providers.Memory;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A <see cref="CacheStack"/> builder, allowing the configuration of cache layers and extensions.
/// </summary>
public interface ICacheStackBuilder
{
	/// <summary>
	/// A collection of cache layers the <see cref="CacheStack"/> will use in order of priority.
	/// </summary>
	ICollection<ICacheLayer> CacheLayers { get; }
	/// <summary>
	/// A collection of the cache extensions the <see cref="CacheStack"/> will use.
	/// </summary>
	ICollection<ICacheExtension> Extensions { get; }
}

internal sealed class CacheStackBuilder : ICacheStackBuilder
{
	/// <inheritdoc/>
	public ICollection<ICacheLayer> CacheLayers { get; } = new List<ICacheLayer>();
	/// <inheritdoc/>
	public ICollection<ICacheExtension> Extensions { get; } = new List<ICacheExtension>();
}


/// <summary>
/// Microsoft <see cref="IServiceCollection"/> extensions for Cache Tower
/// </summary>
public static class ServiceCollectionExtensions
{
	public static void AddCacheStack(this IServiceCollection services, Action<ICacheStackBuilder> configureBuilder)
	{
		var builder = new CacheStackBuilder();
		configureBuilder(builder);
		services.AddSingleton<ICacheStack>(sp => new CacheStack(
			builder.CacheLayers.ToArray(),
			builder.Extensions.ToArray()
		));
	}

	public static void AddCacheStack<TContext>(this IServiceCollection services, Action<ICacheStackBuilder> configureBuilder)
	{
		var builder = new CacheStackBuilder();
		configureBuilder(builder);
		services.AddSingleton<ICacheStack<TContext>>(sp => new CacheStack<TContext>(
			new ServiceProviderContextActivator(sp),
			builder.CacheLayers.ToArray(),
			builder.Extensions.ToArray()
		));
	}

	public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheContextActivator contextActivator, Action<ICacheStackBuilder> configureBuilder)
	{
		var builder = new CacheStackBuilder();
		configureBuilder(builder);
		services.AddSingleton<ICacheStack<TContext>>(sp => new CacheStack<TContext>(
			contextActivator,
			builder.CacheLayers.ToArray(),
			builder.Extensions.ToArray()
		));
	}

	public static ICacheStackBuilder AddMemoryCacheLayer(this ICacheStackBuilder builder)
	{
		builder.CacheLayers.Add(new MemoryCacheLayer());
		return builder;
	}

	public static ICacheStackBuilder AddFileCacheLayer(this ICacheStackBuilder builder, FileCacheLayerOptions options)
	{
		builder.CacheLayers.Add(new FileCacheLayer(options));
		return builder;
	}

	public static ICacheStackBuilder WithCleanupFrequency(this ICacheStackBuilder builder, TimeSpan frequency)
	{
		builder.Extensions.Add(new AutoCleanupExtension(frequency));
		return builder;
	}

	private static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
	{
		foreach (var value in values)
		{
			collection.Add(value);
		}
	}

	/// <summary>
	/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the given layers and automatic cleanup frequency (via <see cref="AutoCleanupExtension"/>).
	/// </summary>
	/// <param name="services">The services collection to add the dependencies to.</param>
	/// <param name="layers">The cache layers to use.</param>
	/// <param name="cleanupFrequency">The frequency at which cache stack cleanup will be performed.</param>
	[Obsolete("Use service collection extension with builder pattern instead. This will be removed in a future version.")]
	public static void AddCacheStack(this IServiceCollection services, ICacheLayer[] layers, TimeSpan cleanupFrequency)
	{
		services.AddCacheStack(builder =>
		{
			builder.CacheLayers.AddRange(layers);
			builder.WithCleanupFrequency(cleanupFrequency);
		});
	}

	/// <summary>
	/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the specified layers and extensions.
	/// </summary>
	/// <param name="services">The services collection to add the dependencies to.</param>
	/// <param name="layers">The cache layers to use.</param>
	/// <param name="extensions">The cache extensions to use.</param>
	[Obsolete("Use service collection extension with builder pattern instead. This will be removed in a future version.")]
	public static void AddCacheStack(this IServiceCollection services, ICacheLayer[] layers, ICacheExtension[] extensions)
	{
		services.AddCacheStack(builder =>
		{
			builder.CacheLayers.AddRange(layers);
			builder.Extensions.AddRange(extensions);
		});
	}

	/// <summary>
	/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the specified layers and extensions.
	/// An implementation factory of <typeparamref name="TContext"/> is built using the <see cref="IServiceProvider"/> established when instantiating the <see cref="CacheStack{TContext}"/>.
	/// </summary>
	/// <param name="services">The services collection to add the dependencies to.</param>
	/// <param name="layers">The cache layers to use.</param>
	/// <param name="extensions">The cache extensions to use.</param>
	[Obsolete("Use service collection extension with builder pattern instead. This will be removed in a future version.")]
	public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheLayer[] layers, ICacheExtension[] extensions)
	{
		services.AddCacheStack<TContext>(builder =>
		{
			builder.CacheLayers.AddRange(layers);
			builder.Extensions.AddRange(extensions);
		});
	}

	/// <summary>
	/// Adds a <see cref="CacheStack{TContext}"/> singleton to the specified <see cref="IServiceCollection"/> with the specified <paramref name="contextFactory"/>, layers and extensions.
	/// </summary>
	/// <param name="services">The services collection to add the dependencies to.</param>
	/// <param name="contextFactory">The factory method that will generate a context when <see cref="CacheStack{TContext}.GetOrSetAsync{T}(string, Func{T, TContext, System.Threading.Tasks.Task{T}}, CacheSettings)"/> is called.</param>
	/// <param name="layers">The cache layers to use.</param>
	/// <param name="extensions">The cache extensions to use.</param>
	[Obsolete("Use service collection extension with builder pattern instead. This will be removed in a future version.")]
	public static void AddCacheStack<TContext>(this IServiceCollection services, Func<TContext> contextFactory, ICacheLayer[] layers, ICacheExtension[] extensions)
	{
		services.AddCacheStack<TContext>(new FuncCacheContextActivator<TContext>(contextFactory), builder =>
		{
			builder.CacheLayers.AddRange(layers);
			builder.Extensions.AddRange(extensions);
		});
	}
}