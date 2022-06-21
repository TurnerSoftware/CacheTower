using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CacheTower.Tests;

[TestClass]
public class CacheContextActivatorTests
{
	private class MyResolvableType
	{
		public string Description { get; set; }
	}

	private void AssertActivator(ICacheContextActivator activator, string expectedDescription)
	{
		using var scope = activator.BeginScope();
		var result = (MyResolvableType)scope.Resolve(typeof(MyResolvableType));
		Assert.AreEqual(expectedDescription, result.Description);
	}

	[TestMethod]
	public void ServiceProviderContextActivator_Resolves()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped(p => new MyResolvableType
		{
			Description = "ServiceProvider"
		});
		var serviceProvider = serviceCollection.BuildServiceProvider();
		var activator = new ServiceProviderContextActivator(serviceProvider);

		AssertActivator(activator, "ServiceProvider");
	}

	[TestMethod]
	public void FuncCacheContextActivator_Resolves()
	{
		var activator = new FuncCacheContextActivator<MyResolvableType>(() => new MyResolvableType
		{
			Description = "FuncCache"
		});

		AssertActivator(activator, "FuncCache");
	}
}
