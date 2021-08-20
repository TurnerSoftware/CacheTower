namespace CacheTower
{
	/// <summary>
	/// Activator for creating a scope when resolving a <see cref="CacheStack{TContext}"/> context.
	/// </summary>
	public interface ICacheContextActivator
	{
		/// <summary>
		/// Begin a scope, and return the <see cref="ICacheContextScope"/> for resolving from 
		/// </summary>
		/// <returns>A scope for the <see cref="CacheStack{TContext}"/>.</returns>
		ICacheContextScope BeginScope();
	}
}