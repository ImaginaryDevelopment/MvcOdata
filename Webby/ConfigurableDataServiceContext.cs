namespace Webby
{
	using System;
	using System.Data.Services.Client;

	public abstract class ConfigurableDataServiceContext : DataServiceContext
	{
		protected ConfigurableDataServiceContext(Uri uri)
			: base(uri)
		{
			this.IgnoreMissingProperties = true;
			this.IgnoreResourceNotFoundException = true;
			this.SendingRequest += ConfigurableDataServiceContextSendingRequest;
		}

		private static void ConfigurableDataServiceContextSendingRequest(object sender, SendingRequestEventArgs e)
		{
			
		}
	}
}