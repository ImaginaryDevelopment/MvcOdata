namespace Webby
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.Xml;

	public class BindingFactory
	{
		public Binding CreateBinding(ProxyConfiguration configuration)
		{
			var binding = (Binding)Activator.CreateInstance(configuration.BindingType);
			if (binding is WSHttpBinding)
			{
				var b = ((WSHttpBinding)binding);

				b.Security.Mode = SecurityMode.None;
				b.MaxReceivedMessageSize = configuration.MaxReceivedMessageSize;
				b.MessageEncoding = configuration.MessageEncoding;

				ConfigureReaderQuotas(configuration, b.ReaderQuotas);
			}
			else if (binding is NetTcpBinding)
			{
				var b = ((NetTcpBinding)binding);

				b.Security.Mode = SecurityMode.None;
				b.MaxReceivedMessageSize = configuration.MaxReceivedMessageSize;

				ConfigureReaderQuotas(configuration, b.ReaderQuotas);
			}
			else if (binding is BasicHttpBinding)
			{
				var b = ((BasicHttpBinding)binding);
				b.Security.Mode = BasicHttpSecurityMode.None;
				b.MaxReceivedMessageSize = configuration.MaxReceivedMessageSize;
				b.MessageEncoding = configuration.MessageEncoding;

				ConfigureReaderQuotas(configuration, b.ReaderQuotas);
			}
			else if (binding is WebHttpBinding)
			{
				var b = ((WebHttpBinding)binding);

				b.Security.Mode = WebHttpSecurityMode.None;
				b.MaxReceivedMessageSize = configuration.MaxReceivedMessageSize;

				ConfigureReaderQuotas(configuration, b.ReaderQuotas);
			}

			binding.ReceiveTimeout = configuration.ReceiveTimeout;
			binding.SendTimeout = configuration.SendTimeout;
			binding.OpenTimeout = configuration.OpenTimeout;

			return binding;
		}

		private static void ConfigureReaderQuotas(ProxyConfiguration configuration, XmlDictionaryReaderQuotas readerQuotas)
		{
			readerQuotas.MaxArrayLength = configuration.MaxArrayLength;
			readerQuotas.MaxBytesPerRead = configuration.MaxBytesPerRead;
			readerQuotas.MaxDepth = configuration.MaxDepth;
			readerQuotas.MaxNameTableCharCount = configuration.MaxNameTableCharCount;
			readerQuotas.MaxStringContentLength = configuration.MaxStringContentLength;
		}
	}
}