using System;
using Microsoft.Extensions.DependencyInjection;

namespace CacheTower;

internal readonly struct ServiceProviderContextActivator : ICacheContextActivator
{
	private readonly IServiceProvider ServiceProvider;

	public ServiceProviderContextActivator(IServiceProvider serviceProvider)
	{
		ServiceProvider = serviceProvider;
	}

	public ICacheContextScope BeginScope()
	{
		return new ServiceProviderContextScope(ServiceProvider.CreateScope());
	}
}

internal readonly struct ServiceProviderContextScope : ICacheContextScope
{
	private readonly IServiceScope ServiceScope;

	public ServiceProviderContextScope(IServiceScope serviceScope)
	{
		ServiceScope = serviceScope;
	}

	public object Resolve(Type type)
	{
		return ServiceScope.ServiceProvider.GetRequiredService(type);
	}

	public void Dispose()
	{
		ServiceScope.Dispose();
	}
}