namespace Webby
{
	using System;
	using System.ServiceModel;

	public class ServiceLocatorServiceHost : ServiceHost
	{
		public ServiceLocatorServiceHost()
		{
		}

		public ServiceLocatorServiceHost(Type serviceType, params Uri[] baseAddresses)
			: base(serviceType, baseAddresses)
		{
		}

		protected override void OnOpening()
		{
			//this.Description.Behaviors.Add(new ServiceLocatorServiceBehavior());
			base.OnOpening();
		}
	}
}