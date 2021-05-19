namespace CacheTower
{
	/// <summary>
	/// Activator for creating a scope when resolving <typeparamref name="TContext"/>
	/// </summary>
	public interface ICacheContextActivator
	{
		/// <summary>
		/// Begin a scope, and return the <typeparamref name="ICacheContextScope"/> for resolving from 
		/// </summary>
		/// <returns><typeparamref name="ICacheContextScope"/></returns>
		ICacheContextScope BeginScope();
	}
}