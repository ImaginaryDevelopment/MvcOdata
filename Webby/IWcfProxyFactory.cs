namespace Webby
{
	using Contracts;

	public interface IWcfProxyFactory<out TService>
		where TService : class
	{
		TService CreateProxy(IConfig config,string instance = null);
	}
}