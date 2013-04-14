namespace Webby
{
	using System;
	using System.Collections.Generic;
	using System.Data.Services;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Description;

	using Contracts;

	public class VirtualWcfFactory : ServiceHostFactory
	{
		private static readonly IDictionary<string, Type> TypeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

		private static readonly object Lock = new object();

		private static Type GetType(string typeString)
		{
			Type serviceType;

			if (TypeMap.TryGetValue(typeString, out serviceType))
			{
				return serviceType;
			}

			lock (Lock)
			{
				if (!TypeMap.ContainsKey(typeString))
				{
					System.Diagnostics.Debugger.Break();
					serviceType = Type.GetType(typeString, true, true);

					TypeMap.Add(typeString, serviceType);
				}
				else
				{
					serviceType = TypeMap[typeString];
				}

				return serviceType;
			}
		}

		private static void ConfigureService(Type iface, ServiceHost host)
		{
			var proxyConfig = ProxyConfiguration.Create(new Config(), iface.Name.Substring(1));

			//host.Authorization.ServiceAuthorizationManager = new PaySpanServiceAuthorizationManager();

			var ep = host.AddServiceEndpoint(iface, new BindingFactory().CreateBinding(proxyConfig), string.Empty);

			//ep.Behaviors.Add(new ContextPropagator());

			if (proxyConfig.BindingType == typeof(WebHttpBinding))
			{
				ep.Behaviors.Add(new WebHttpBehavior());
			}

			foreach (var op in ep.Contract.Operations)
			{
				var dataContract = op.Behaviors.Find<DataContractSerializerOperationBehavior>();

				if (dataContract == null)
				{
					dataContract = new DataContractSerializerOperationBehavior(op) { MaxItemsInObjectGraph = proxyConfig.MaxItemsInObjectGraph };
					op.Behaviors.Add(dataContract);
				}
				else
				{
					dataContract.MaxItemsInObjectGraph = proxyConfig.MaxItemsInObjectGraph;
				}
			}

			var serviceDebugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();

			if (serviceDebugBehavior == null)
			{
				serviceDebugBehavior = new ServiceDebugBehavior { IncludeExceptionDetailInFaults = proxyConfig.IncludeExceptionDetailInFaults };
				host.Description.Behaviors.Add(serviceDebugBehavior);
			}
			else
			{
				serviceDebugBehavior.IncludeExceptionDetailInFaults = proxyConfig.IncludeExceptionDetailInFaults;
			}

			var metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();

			if (metadataBehavior == null)
			{
				metadataBehavior = new ServiceMetadataBehavior { HttpGetEnabled = true };
				host.Description.Behaviors.Add(metadataBehavior);
			}
			else
			{
				metadataBehavior.HttpGetEnabled = true;
			}
		}

		public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
		{
			var serviceType = GetType(constructorString);

			ServiceHost host;

			if (serviceType.IsDataService())
			{
				host = new WcfDataServicesHost(serviceType, baseAddresses);
			}
			else
			{
				host = new ServiceLocatorServiceHost(serviceType, baseAddresses);
			}

			foreach (var iface in serviceType.GetInterfaces().Where(i => i != typeof(IRequestHandler)))
			{
				var attr = (ServiceContractAttribute)Attribute.GetCustomAttribute(iface, typeof(ServiceContractAttribute));

				if (attr == null)
				{
					continue;
				}

				ConfigureService(iface, host);
				break;
			}

			return host;
		}
	}
}