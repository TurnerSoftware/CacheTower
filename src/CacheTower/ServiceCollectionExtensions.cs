using System;
using System.Collections.Generic;
using System.Linq;
using CacheTower;
using CacheTower.Extensions;
using CacheTower.Providers.FileSystem;
using CacheTower.Providers.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

internal sealed class CacheStackBuilder : ICacheStackBuilder
{
	/// <inheritdoc/>
	public IList<ICacheLayer> CacheLayers { get; } = new List<ICacheLayer>();
	/// <inheritdoc/>
	public IList<ICacheExtension> Extensions { get; } = new List<ICacheExtension>();
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
			builder.CacheLayers.ToArray(),
			builder.Extensions.ToArray()
		);
	}

	private static ICacheStack<TContext> BuildCacheStack<TContext>(IServiceProvider provider, ICacheContextActivator contextActivator, Action<IServiceProvider, ICacheStackBuilder> configureBuilder)
	{
		var builder = new CacheStackBuilder();
		configureBuilder(provider, builder);
		ThrowIfInvalidBuilder(builder);
		return new CacheStack<TContext>(
			contextActivator,
			builder.CacheLayers.ToArray(),
			builder.Extensions.ToArray()
		);
	}

	/// <inheritdoc cref="AddCacheStack(IServiceCollection, Action{IServiceProvider, ICacheStackBuilder})"/>
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
		services.TryAddSingleton<NamedCacheStackLookup<ICacheStack>>();
		services.TryAddSingleton<ICacheStackAccessor, CacheStackAccessor>();
		services.AddSingleton(provider =>
		{
			return new NamedCacheStackProvider<ICacheStack>(name, provider =>
			{
				return BuildCacheStack(provider, configureBuilder);
			});
		});
	}

	/// <inheritdoc cref="AddCacheStack{TContext}(IServiceCollection, Action{IServiceProvider, ICacheStackBuilder})"/>
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
	public static void AddCacheStack<TContext>(this IServiceCollection services, Action<IServiceProvider, ICacheStackBuilder> configureBuilder)
	{
		services.AddSingleton(provider => BuildCacheStack<TContext>(provider, new ServiceProviderContextActivator(provider), configureBuilder));
	}

	/// <summary>
	/// Adds a <see cref="ICacheStackAccessor{TContext}"/> to the service collection and configures a named <see cref="CacheStack{TContext}"/>.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="name">The name of the <see cref="CacheStack"/> to configure.</param>
	/// <param name="configureBuilder">The builder to configure the <see cref="CacheStack"/>.</param>
	public static void AddCacheStack<TContext>(this IServiceCollection services, string name, Action<IServiceProvider, ICacheStackBuilder> configureBuilder)
	{
		services.TryAddSingleton<NamedCacheStackLookup<ICacheStack<TContext>>>();
		services.TryAddSingleton<ICacheStackAccessor<TContext>, CacheStackAccessor<TContext>>();
		services.AddSingleton(provider =>
		{
			return new NamedCacheStackProvider<ICacheStack<TContext>>(name, provider =>
			{
				return BuildCacheStack<TContext>(provider, new ServiceProviderContextActivator(provider), configureBuilder);
			});
		});
	}

	/// <inheritdoc cref="AddCacheStack{TContext}(IServiceCollection, ICacheContextActivator, Action{IServiceProvider, ICacheStackBuilder})"/>
	public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheContextActivator contextActivator, Action<ICacheStackBuilder> configureBuilder)
	{
		services.AddCacheStack<TContext>(contextActivator, (provider, builder) => configureBuilder(builder));
	}

	/// <summary>
	/// Adds a <see cref="CacheStack{TContext}"/> to the service collection with the specified <paramref name="contextActivator"/>.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <param name="services"></param>
	/// <param name="contextActivator">The activator to instantiate the <typeparamref name="TContext"/> during cache refreshing.</param>
	/// <param name="configureBuilder">The builder to configure the <see cref="CacheStack"/>.</param>
	public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheContextActivator contextActivator, Action<IServiceProvider, ICacheStackBuilder> configureBuilder)
	{
		services.AddSingleton<ICacheStack>(provider => BuildCacheStack<TContext>(provider, contextActivator, configureBuilder));
	}

	/// <summary>
	/// Adds a <see cref="ICacheStackAccessor{TContext}"/> to the service collection and configures a named <see cref="CacheStack{TContext}"/>.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="name">The name of the <see cref="CacheStack"/> to configure.</param>
	/// <param name="contextActivator">The activator to instantiate the <typeparamref name="TContext"/> during cache refreshing.</param>
	/// <param name="configureBuilder">The builder to configure the <see cref="CacheStack"/>.</param>
	public static void AddCacheStack<TContext>(this IServiceCollection services, string name, ICacheContextActivator contextActivator, Action<IServiceProvider, ICacheStackBuilder> configureBuilder)
	{
		services.TryAddSingleton<NamedCacheStackLookup<ICacheStack<TContext>>>();
		services.TryAddSingleton<ICacheStackAccessor<TContext>, CacheStackAccessor<TContext>>();
		services.AddSingleton(provider =>
		{
			return new NamedCacheStackProvider<ICacheStack<TContext>>(name, provider =>
			{
				return BuildCacheStack<TContext>(provider, contextActivator, configureBuilder);
			});
		});
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