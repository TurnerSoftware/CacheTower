using System;
using System.Threading.Tasks;

namespace CacheTower.Monitoring
{
	internal static class TimingUtils
	{
		public static async Task Time(Func<DisposableTimer, Task> func)
		{
			using var timer = new DisposableTimer();
			await func(timer);
		}		
		
		public static async Task<T> Time<T>(Func<DisposableTimer, Task<T>> func)
		{
			using var timer = new DisposableTimer();
			return await func(timer);
		}
	}
}