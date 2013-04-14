using System;
using System.Collections.Generic;

using Microsoft.Practices.ServiceLocation;

using Ninject;

namespace Webby
{
	public class NinjectServiceLocator : IServiceLocator
	{
		private readonly IKernel _kernel;

		public NinjectServiceLocator(IKernel kernel)
		{
			_kernel = kernel;
		}

		public object GetService(Type serviceType)
		{
			return _kernel.GetService(serviceType);
		}

		public object GetInstance(Type serviceType)
		{
			return _kernel.Get(serviceType);
		}

		public object GetInstance(Type serviceType, string key)
		{
			return _kernel.Get(serviceType, key);
		}

		public IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return _kernel.GetAll(serviceType);
		}

		public TService GetInstance<TService>()
		{
			return (TService)_kernel.Get(typeof(TService));
		}

		public TService GetInstance<TService>(string key)
		{
			return (TService)_kernel.Get(typeof(TService), key);
		}

		public IEnumerable<TService> GetAllInstances<TService>()
		{
			return (IEnumerable<TService>)_kernel.GetAll(typeof(TService));
		}
	}
}
