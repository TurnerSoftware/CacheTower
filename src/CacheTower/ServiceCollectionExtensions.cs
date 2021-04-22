using System;
using CacheTower;
using CacheTower.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Microsoft <see cref="IServiceCollection"/> extensions for Cache Tower
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the given layers and automatic cleanup frequency (via <see cref="AutoCleanupExtension"/>).
		/// </summary>
		/// <param name="services">The services collection to add the dependencies to.</param>
		/// <param name="layers">The cache layers to use.</param>
		/// <param name="cleanupFrequency">The frequency at which cache stack cleanup will be performed.</param>
		public static void AddCacheStack(this IServiceCollection services, ICacheLayer[] layers, TimeSpan cleanupFrequency)
		{
			services.AddSingleton<ICacheStack>(new CacheStack(layers, new[] { new AutoCleanupExtension(cleanupFrequency) }));
		}

		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the specified layers and extensions.
		/// </summary>
		/// <param name="services">The services collection to add the dependencies to.</param>
		/// <param name="layers">The cache layers to use.</param>
		/// <param name="extensions">The cache extensions to use.</param>
		public static void AddCacheStack(this IServiceCollection services, ICacheLayer[] layers, ICacheExtension[] extensions)
		{
			services.AddSingleton<ICacheStack>(new CacheStack(layers, extensions));
		}

		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the specified layers and extensions.
		/// An implementation factory of <typeparamref name="TContext"/> is built using the <see cref="IServiceProvider"/> established when instantiating the <see cref="CacheStack{TContext}"/>.
		/// </summary>
		/// <param name="services">The services collection to add the dependencies to.</param>
		/// <param name="layers">The cache layers to use.</param>
		/// <param name="extensions">The cache extensions to use.</param>
		public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheLayer[] layers, ICacheExtension[] extensions)
		{
			services.AddSingleton<ICacheStack<TContext>, CacheStack<TContext>>(sp => {
				TContext contextFactory() => sp.GetRequiredService<TContext>();
				return new CacheStack<TContext>(contextFactory, layers, extensions);
			});
		}

		/// <summary>
		/// Adds a <see cref="CacheStack{TContext}"/> singleton to the specified <see cref="IServiceCollection"/> with the specified <paramref name="contextFactory"/>, layers and extensions.
		/// </summary>
		/// <param name="services">The services collection to add the dependencies to.</param>
		/// <param name="contextFactory">The factory method that will generate a context when <see cref="CacheStack{TContext}.GetOrSetAsync{T}(string, Func{T, TContext, System.Threading.Tasks.Task{T}}, CacheSettings)"/> is called.</param>
		/// <param name="layers">The cache layers to use.</param>
		/// <param name="extensions">The cache extensions to use.</param>
		public static void AddCacheStack<TContext>(this IServiceCollection services, Func<TContext> contextFactory, ICacheLayer[] layers, ICacheExtension[] extensions)
		{
			services.AddSingleton<ICacheStack<TContext>, CacheStack<TContext>>(sp => {
				return new CacheStack<TContext>(contextFactory, layers, extensions);
			});
		}
	}
}
