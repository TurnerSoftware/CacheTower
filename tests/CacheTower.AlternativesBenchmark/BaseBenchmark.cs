using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CacheTower.AlternativesBenchmark
{
	public abstract class BaseBenchmark
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void LoopAction(int iterations, Action action)
		{
			for (var i = 0; i < iterations; i++)
			{
				action();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected async Task LoopActionAsync(int iterations, Func<Task> action)
		{
			for (var i = 0; i < iterations; i++)
			{
				await action();
			}
		}
	}
}
