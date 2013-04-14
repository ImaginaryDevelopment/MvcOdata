namespace Webby
{
	using System.ServiceModel;
	using System.ServiceModel.Description;

	public abstract class ConfigurableExceptionHandlingProxyBase<T> : ExceptionHandlingProxyBase<T>
		where T : class
	{
		private readonly ProxyConfiguration _proxyConfiguration;

		protected ConfigurableExceptionHandlingProxyBase(ProxyConfiguration proxyConfiguration)
		{
			this._proxyConfiguration = proxyConfiguration;

			if (this._proxyConfiguration.UseEndpointConfiguration)
			{
				Initialize(this._proxyConfiguration.EndpointConfigurationName, this._proxyConfiguration.Address);
			}
			else
			{
				Initialize(new BindingFactory().CreateBinding(this._proxyConfiguration), this._proxyConfiguration.Address);
			}

			this.ConfigureChannelFactory(this.ChannelFactory);
			//Endpoint.Behaviors.Add(new ContextPropagator());

			Open();
			InnerChannel.OperationTimeout = proxyConfiguration.OperationTimeout;
		}

		private void ConfigureChannelFactory(ChannelFactory<T> factory)
		{
			foreach (var op in factory.Endpoint.Contract.Operations)
			{
				var dataContract = op.Behaviors.Find<DataContractSerializerOperationBehavior>();

				if (dataContract != null)
				{
					dataContract.MaxItemsInObjectGraph = this._proxyConfiguration.MaxItemsInObjectGraph;
				}
			}
		}
	}
}