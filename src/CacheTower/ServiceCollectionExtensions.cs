using System;
using CacheTower;
using CacheTower.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the given layers and automatic cleanup frequency (via <see cref="AutoCleanupExtension"/>).
		/// </summary>
		/// <param name="services"></param>
		/// <param name="layers"></param>
		/// <param name="cleanupFrequency"></param>
		public static void AddCacheStack(this IServiceCollection services, ICacheLayer[] layers, TimeSpan cleanupFrequency)
		{
			services.AddSingleton<ICacheStack>(new CacheStack(layers, new[] { new AutoCleanupExtension(cleanupFrequency) }));
		}

		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the specified layers and extensions.
		/// An implementation factory of <typeparamref name="TContext"/> is built using the <see cref="IServiceProvider"/> established when instantiating the <see cref="CacheStack{TContext}"/>.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="context"></param>
		/// <param name="layers"></param>
		/// <param name="extensions"></param>
		public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheLayer[] layers, ICacheExtension[] extensions)
		{
			services.AddSingleton<ICacheStack<TContext>, CacheStack<TContext>>(sp => {
				TContext contextFactory() => sp.GetRequiredService<TContext>();
				return new CacheStack<TContext>(contextFactory, layers, extensions);
			});
		}

		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the specified <paramref name="contextFactory"/>, layers and extensions.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="context"></param>
		/// <param name="layers"></param>
		/// <param name="extensions"></param>
		public static void AddCacheStack<TContext>(this IServiceCollection services, Func<TContext> contextFactory, ICacheLayer[] layers, ICacheExtension[] extensions)
		{
			services.AddSingleton<ICacheStack<TContext>, CacheStack<TContext>>(sp => {
				return new CacheStack<TContext>(contextFactory, layers, extensions);
			});
		}
	}
}
