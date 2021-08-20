using System;

namespace CacheTower
{
	/// <remarks>
	/// A scope for resolving a <see cref="CacheStack{TContext}"/> context.
	/// </remarks>
	/// <inheritdoc/>
	public interface ICacheContextScope : IDisposable
	{
		/// <summary>
		/// Function for resolving a type to a concrete implementation
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		object Resolve(Type type);
	}
}