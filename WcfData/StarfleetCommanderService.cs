
using System;
using System.Data.Services;
using System.Data.Services.Common;

using Microsoft.Practices.ServiceLocation;

using Contracts;
using Webby;

namespace WcfData
{
    [System.ServiceModel.ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class StarfleetCommanderService : WcfDataService<IStarfleetCommander>
    {
        public static void InitializeService(DataServiceConfiguration config)
        {
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
            config.SetEntitySetAccessRule(LinqOp.PropertyOf<IStarfleetCommander>(p => p.Universes).Name, EntitySetRights.AllRead);

            config.UseVerboseErrors = true;
        }

        protected override IStarfleetCommander CreateDataSource()
        {
            return ServiceLocator.Current.GetInstance<IStarfleetCommander>();
        }
    }
}
