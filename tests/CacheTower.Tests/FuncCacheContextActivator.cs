using System;

namespace CacheTower.Tests;

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
