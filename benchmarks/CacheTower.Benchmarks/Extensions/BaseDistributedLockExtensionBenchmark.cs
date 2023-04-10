using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace CacheTower.Benchmarks.Extensions;

public abstract class BaseDistributedLockExtensionBenchmark : BaseExtensionsBenchmark
{
	[Benchmark]
	public async Task AwaitAccessAndRelease()
	{
		var extension = CacheExtension as IDistributedLockExtension;
		await using var _ = await extension.AwaitAccessAsync("RefreshValue");
	}
}
