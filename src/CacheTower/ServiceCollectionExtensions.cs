using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CacheTower;
using CacheTower.Extensions;
using CacheTower.Providers.FileSystem;
using CacheTower.Providers.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A <see cref="CacheStack"/> builder, allowing the configuration of cache layers and extensions.
/// </summary>
/// <remarks>
/// The order of cache layers added is important - the first layer you add will be the first layer hit for a cache lookup.
/// For example, if you want a memory cache layer and a file cache layer, add the memory cache layer first.
/// </remarks>
public interface ICacheStackBuilder
{
	/// <summary>
	/// A collection of cache layers the <see cref="CacheStack"/> will use in order of priority.
	/// </summary>
	IList<ICacheLayer> CacheLayers { get; }
	/// <summary>
	/// A collection of the cache extensions the <see cref="CacheStack"/> will use.
	/// </summary>
	IList<ICacheExtension> Extensions { get; }
}

/// <inheritdoc/>
/// <typeparam name="TContext">The type of context that is passed during the cache entry generation process.</typeparam>
public interface ICacheStackBuilder<TContext> : ICacheStackBuilder
{
	/// <summary>
	/// The activator that is used to resolve <typeparamref name="TContext"/> for the cache entry generation process.
	/// </summary>
	/// <remarks>
	/// The default activator uses the current service collection as a means to instantiate <typeparamref name="TContext"/>.
	/// </remarks>
	public ICacheContextActivator CacheContextActivator { get; set; }
}

internal class CacheStackBuilder : ICacheStackBuilder
{
	/// <inheritdoc/>
	public IList<ICacheLayer> CacheLayers { get; } = new List<ICacheLayer>();
	/// <inheritdoc/>
	public IList<ICacheExtension> Extensions { get; } = new List<ICacheExtension>();
}

internal sealed class CacheStackBuilder<TContext> : CacheStackBuilder, ICacheStackBuilder<TContext>
{
	/// <inheritdoc/>
	public ICacheContextActivator CacheContextActivator { get; set; }

	public CacheStackBuilder(ICacheContextActivator cacheContextActivator)
	{
		CacheContextActivator = cacheContextActivator;
	}
}

/// <summary>
/// Microsoft <see cref="IServiceCollection"/> extensions for Cache Tower.
/// </summary>
public static class ServiceCollectionExtensions
{
	private static void ThrowIfInvalidBuilder(ICacheStackBuilder builder)
	{
		if (builder.CacheLayers.Count == 0)
		{
			throw new InvalidOperationException("No cache layers have been configured");
		}
	}

	private static ICacheStack BuildCacheStack(IServiceProvider provider, Action<IServiceProvider, ICacheStackBuilder> configureBuilder)
	{
		var builder = new CacheStackBuilder();
		configureBuilder(provider, builder);
		ThrowIfInvalidBuilder(builder);
		return new CacheStack(
			provider.GetService<ILogger<CacheStack>>(),
			builder.CacheLayers.ToArray(),
			builder.Extensions.ToArray()
		);
	}

	private static ICacheStack<TContext> BuildCacheStack<TContext>(IServiceProvider provider, Action<IServiceProvider, ICacheStackBuilder<TContext>> configureBuilder)
	{
		var builder = new CacheStackBuilder<TContext>(new ServiceProviderContextActivator(provider));
		configureBuilder(provider, builder);
		ThrowIfInvalidBuilder(builder);
		return new CacheStack<TContext>(
			provider.GetService<ILogger<CacheStack>>(),
			builder.CacheContextActivator,
			builder.CacheLayers.ToArray(),
			builder.Extensions.ToArray()
		);
	}

	/// <inheritdoc cref="AddCacheStack(IServiceCollection, Action{IServiceProvider, ICacheStackBuilder})"/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void AddCacheStack(this IServiceCollection services, Action<ICacheStackBuilder> configureBuilder)
	{
		services.AddCacheStack((serviceProvider, builder) => configureBuilder(builder));
	}

	/// <summary>
	/// Adds a <see cref="CacheStack"/> to the service collection.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="configureBuilder">The builder to configure the <see cref="CacheStack"/>.</param>
	public static void AddCacheStack(this IServiceCollection services, Action<IServiceProvider, ICacheStackBuilder> configureBuilder)
	{
		services.AddSingleton(provider => BuildCacheStack(provider, configureBuilder));
	}

	/// <summary>
	/// Adds a <see cref="ICacheStackAccessor"/> to the service collection and configures a named <see cref="CacheStack"/>.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="name">The name of the <see cref="CacheStack"/> to configure.</param>
	/// <param name="configureBuilder">The builder to configure the <see cref="CacheStack"/>.</param>
	public static void AddCacheStack(this IServiceCollection services, string name, Action<IServiceProvider, ICacheStackBuilder> configureBuilder)
	{
		services.TryAddSingleton<NamedCacheStackLookup>();
		services.TryAddSingleton<ICacheStackAccessor, CacheStackAccessor>();
		services.AddSingleton(provider =>
		{
			return new NamedCacheStackProvider(name, provider =>
			{
				return BuildCacheStack(provider, configureBuilder);
			});
		});
	}

	/// <inheritdoc cref="AddCacheStack{TContext}(IServiceCollection, Action{IServiceProvider, ICacheStackBuilder{TContext}})"/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void AddCacheStack<TContext>(this IServiceCollection services, Action<ICacheStackBuilder> configureBuilder)
	{
		services.AddCacheStack<TContext>((provider, builder) => configureBuilder(builder));
	}

	/// <summary>
	/// Adds a <see cref="CacheStack{TContext}"/> to the service collection.
	/// </summary>
	/// <remarks>
	/// The <typeparamref name="TContext"/> will be provided by the service collection for every cache refresh.
	/// </remarks>
	/// <typeparam name="TContext"></typeparam>
	/// <param name="services"></param>
	/// <param name="configureBuilder">The builder to configure the <see cref="CacheStack"/>.</param>
	public static void AddCacheStack<TContext>(this IServiceCollection services, Action<IServiceProvider, ICacheStackBuilder<TContext>> configureBuilder)
	{
		services.AddSingleton(provider => BuildCacheStack(provider, configureBuilder));
	}

	/// <summary>
	/// Adds a <see cref="ICacheStackAccessor{TContext}"/> to the service collection and configures a named <see cref="CacheStack{TContext}"/>.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="name">The name of the <see cref="CacheStack"/> to configure.</param>
	/// <param name="configureBuilder">The builder to configure the <see cref="CacheStack"/>.</param>
	public static void AddCacheStack<TContext>(this IServiceCollection services, string name, Action<IServiceProvider, ICacheStackBuilder<TContext>> configureBuilder)
	{
		services.TryAddSingleton<NamedCacheStackLookup>();
		services.TryAddSingleton<ICacheStackAccessor, CacheStackAccessor>();
		services.TryAddSingleton<ICacheStackAccessor<TContext>, CacheStackAccessor<TContext>>();
		services.AddSingleton(provider =>
		{
			return new NamedCacheStackProvider(name, provider =>
			{
				return BuildCacheStack(provider, configureBuilder);
			});
		});
	}

	/// <summary>
	/// Adds a <see cref="CacheStack{TContext}"/> to the service collection with the specified <paramref name="contextActivator"/>.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <param name="services"></param>
	/// <param name="contextActivator">The activator to instantiate the <typeparamref name="TContext"/> during cache refreshing.</param>
	/// <param name="configureBuilder">The builder to configure the <see cref="CacheStack"/>.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheContextActivator contextActivator, Action<ICacheStackBuilder> configureBuilder)
	{
		services.AddSingleton(provider => BuildCacheStack<TContext>(provider, (provider, builder) =>
		{
			builder.CacheContextActivator = contextActivator;
			configureBuilder(builder);
		}));
	}

	/// <summary>
	/// Adds a <see cref="MemoryCacheLayer"/> to the <see cref="CacheStack"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static ICacheStackBuilder AddMemoryCacheLayer(this ICacheStackBuilder builder)
	{
		builder.CacheLayers.Add(new MemoryCacheLayer());
		return builder;
	}

	/// <summary>
	/// Adds a <see cref="FileCacheLayer"/> to the <see cref="CacheStack"/> with the specified <paramref name="options"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="options">The <see cref="FileCacheLayer"/> options for configuring directory, serializer and more.</param>
	/// <returns></returns>
	public static ICacheStackBuilder AddFileCacheLayer(this ICacheStackBuilder builder, FileCacheLayerOptions options)
	{
		builder.CacheLayers.Add(new FileCacheLayer(options));
		return builder;
	}

	/// <summary>
	/// Adds the <see cref="AutoCleanupExtension"/> to the <see cref="CacheStack"/> with the specified <paramref name="frequency"/>.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="frequency">How frequent the auto-cleanup process is run.</param>
	/// <returns></returns>
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
}