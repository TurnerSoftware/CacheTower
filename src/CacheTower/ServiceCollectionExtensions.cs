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
		public static void AddCacheStack(this IServiceCollection services, ICacheLayer[] layers, TimeSpan cleanupFrequency)
		{
			services.AddCacheStack(layers, new[] { new AutoCleanupExtension(cleanupFrequency) });
		}

		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the given layers and extensions.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="layers"></param>
		/// <param name="extensions"></param>
		public static void AddCacheStack(this IServiceCollection services, ICacheLayer[] layers, ICacheExtension[] extensions)
		{
			services.AddCacheStack(null, layers, extensions);
		}

		/// <summary>
		/// Adds a <see cref="CacheStack"/> singleton to the specified <see cref="IServiceCollection"/> with the given context, layers and extensions.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="context"></param>
		/// <param name="layers"></param>
		/// <param name="extensions"></param>
		public static void AddCacheStack(this IServiceCollection services, ICacheContext context, ICacheLayer[] layers, ICacheExtension[] extensions)
		{
			services.AddSingleton<ICacheStack, CacheStack>(sp => new CacheStack(context, layers, extensions));
		}
	}
}
