namespace Webby
{
	using System;
	using System.Collections.Generic;

	using Contracts;



	public class DataServiceContextFactory<TService> : IDataServiceContextFactory<TService> where TService : class
	{
		//private readonly IConfig _config;

		public DataServiceContextFactory()
		{
			//this._config = config;
		}

		public TService GetContext(IConfig config, string instance = null)
		{
			return DataServiceContextFactory.GetContext<TService>(config,instance);
		}
	}

	public class DataServiceContextFactory
	{
		private static readonly Dictionary<Type, Type> Types = new Dictionary<Type, Type>();

		private static readonly object TypeLock = new object();

		private static ConfigurableDataServiceContext CreateInstance<TType>(ProxyConfiguration proxyConfig) where TType : class
		{
			var type = GetProxyType<TType>();

			return Activator.CreateInstance(type, new object[] { proxyConfig }) as ConfigurableDataServiceContext;
		}

		private static ConfigurableDataServiceContext CreateInstance<TType>(Uri uri) where TType : class
		{
			var type = GetProxyType<TType>();
			return Activator.CreateInstance(type, new object[] { uri }) as ConfigurableDataServiceContext;
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
					implementationType = new DataServiceContextGenerator<TType>().CreateContextType();
					Types.Add(serviceType, implementationType);
				}
			}

			return implementationType;
		}

		internal static TType GetContext<TType>(IConfig config, string instance = null) where TType : class
		{
			var proxyConfig = ProxyConfiguration.Create(config, typeof(TType).Name.Substring(1)+"Service", instance);

			if (proxyConfig == null)
			{
				return null;
			}

			return CreateInstance<TType>(proxyConfig.Address.Uri) as TType;
		}
	}
}