using System.Text;

namespace Webby
{
	using System;
	using System.Collections.Generic;

	using Contracts;

	public static class WcfProxyFactory
	{
		private static readonly Dictionary<Type, Type> Types = new Dictionary<Type, Type>();

		private static readonly object TypeLock = new object();

		private static ConfigurableExceptionHandlingProxyBase<TType> CreateInstance<TType>(ProxyConfiguration proxyConfig) where TType : class
		{
			var type = GetProxyType<TType>();

			return Activator.CreateInstance(type, new object[] { proxyConfig }) as ConfigurableExceptionHandlingProxyBase<TType>;
		}

		private static Type GetProxyType<TType>() where TType : class
		{
			Type implementationType;
			var serviceType = typeof(TType);

			if (Types.TryGetValue(serviceType, out implementationType))
			{
				return implementationType;
			}

			lock (TypeLock)
			{
				if (!Types.TryGetValue(serviceType, out implementationType))
				{
					implementationType = new WcfProxyGenerator<TType>().CreateProxyType();
					Types.Add(serviceType, implementationType);
				}
			}

			return implementationType;
		}

		internal static TType CreateProxy<TType>(IConfig config, string instance = null) where TType : class
		{
			var proxyConfig = ProxyConfiguration.Create(config, typeof(TType).Name.Substring(1), instance);

			if (proxyConfig == null)
			{
				return null;
			}

			return CreateInstance<TType>(proxyConfig) as TType;
		}
	}

	public class WcfProxyFactory<TService> : IWcfProxyFactory<TService>
				where TService : class
	{
		
		public WcfProxyFactory()
		{
			//this._config = config;
		}

		public TService CreateProxy(IConfig config, string instance = null)
		{
			return WcfProxyFactory.CreateProxy<TService>(config, instance);
		}
	}
}
