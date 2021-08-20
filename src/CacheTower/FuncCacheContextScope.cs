using System;

namespace CacheTower
{
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
}