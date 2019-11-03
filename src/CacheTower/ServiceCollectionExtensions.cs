using System;
using System.Collections.Generic;
using System.Text;
using CacheTower;
using CacheTower.Extensions;
using Microsoft.Extensions.DependencyInjection;

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
		public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheLayer[] layers, TimeSpan cleanupFrequency) where TContext : ICacheContext
		{
			services.AddCacheStack<TContext>(layers, new[] { new AutoCleanupExtension(cleanupFrequency) });
		}

		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the specified <typeparamref name="TContext"/>, layers and extensions.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="context"></param>
		/// <param name="layers"></param>
		/// <param name="extensions"></param>
		public static void AddCacheStack<TContext>(this IServiceCollection services, ICacheLayer[] layers, ICacheExtension[] extensions) where TContext : ICacheContext
		{
			services.AddSingleton<ICacheStack<TContext>, CacheStack<TContext>>(sp => new CacheStack<TContext>(sp.GetRequiredService<TContext>(), layers, extensions));
		}
	}
}
