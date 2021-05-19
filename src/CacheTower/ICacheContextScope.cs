using System;

namespace CacheTower
{
	/// <remarks>
	/// A scope for resolving <typeparamref name="TContext"/>
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