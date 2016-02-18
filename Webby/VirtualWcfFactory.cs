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
        static readonly IDictionary<string, Type> TypeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

        static bool initialized = false;
        static readonly object Lock = new object();

        class GetTypeResult
        {
            public Type Type { get; set; }
            public Exception Exception { get; set; }
            public bool IsSuccess => Exception == null && Type != null;
        }

        static GetTypeResult TryGetType(string typeString)
        {
            try
            {
                var type = Type.GetType(typeString, true, true);
                return new GetTypeResult { Type = type };
            }
            catch (System.IO.FileNotFoundException fEx)
            {
                return new GetTypeResult { Exception = fEx };
            }
            catch (TypeLoadException tEx)
            {
                return new GetTypeResult { Exception = tEx };
            }
        }

        static Type GetType(string typeString)
        {
            Type serviceType;

            if (TypeMap.TryGetValue(typeString, out serviceType))
                return serviceType;

            lock (Lock)
            {
                if (!initialized)
                {
                    initialized = true;
                }

                if (!TypeMap.ContainsKey(typeString))
                {
                    var result = TryGetType(typeString);
                    if (!result.IsSuccess)
                        result = TryGetType(typeString + "Service");
                    if (!result.IsSuccess)
                        result = TryGetType(typeString.Replace(",", "Service,"));
                    if (!result.IsSuccess)
                    {
                        System.Diagnostics.Debugger.Launch();
                        throw new System.TypeLoadException("Failed to load " + typeString, result.Exception);
                    }

                    TypeMap.Add(typeString, result.Type);
                }
                else
                {
                    serviceType = TypeMap[typeString];
                }

                return serviceType;
            }
        }

        static void ConfigureService(Type iface, ServiceHost host)
        {
            var proxyConfig = ProxyConfiguration.Create(new Config(), iface.Name.Substring(1));

            //host.Authorization.ServiceAuthorizationManager = new CompanyNameServiceAuthorizationManager();

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