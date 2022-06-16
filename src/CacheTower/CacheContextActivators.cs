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


internal class FuncCacheContextActivator<TContext> : ICacheContextActivator
{
	private readonly Func<TContext> Resolver;

	public FuncCacheContextActivator(Func<TContext> resolver)
	{
		Resolver = resolver;
	}

	public ICacheContextScope BeginScope()
	{
		return new FuncCacheContextScope<TContext>(Resolver);
	}
}

internal class FuncCacheContextScope<TContext> : ICacheContextScope
{
	private readonly Func<TContext> Resolver;

	public FuncCacheContextScope(Func<TContext> resolver)
	{
		Resolver = resolver;
	}

	public object Resolve(Type type)
	{
		return Resolver()!;
	}

	public void Dispose()
	{
	}
}