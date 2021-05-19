using System;

namespace CacheTower
{
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
}