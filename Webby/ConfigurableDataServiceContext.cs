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
            this.SendingRequest2 += ConfigurableDataServiceContext_SendingRequest2;
            //this.SendingRequest2 += ConfigurableDataServiceContextSendingRequest;
        }

        void ConfigurableDataServiceContext_SendingRequest2(object sender, SendingRequest2EventArgs e)
        {

        }

    }
}