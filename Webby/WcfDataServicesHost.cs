namespace Webby
{
	using System;
	using System.Data.Services;

	public class WcfDataServicesHost : DataServiceHost
	{
		public WcfDataServicesHost(Type type, params Uri[] baseAddresses)
			: base(type, baseAddresses)
		{
		}

		protected override void OnOpening()
		{
			//this.Description.Behaviors.Add(new ServiceLocatorServiceBehavior());
			base.OnOpening();
		}
	}
}