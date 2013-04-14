using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.Dependencies;

using Ninject;
using Ninject.Syntax;
namespace OData
{
	public class NinjectApiResolver : NinjectDependencyScope, IDependencyResolver
	{
		readonly IKernel _kernel;

		public NinjectApiResolver(IKernel kernel)
			: base(kernel)
		{
			this._kernel = kernel;
		}

		public IDependencyScope BeginScope()
		{
			return new NinjectDependencyScope(this._kernel.BeginBlock());
		}
	}

	public class NinjectDependencyScope : IDependencyScope
	{
		IResolutionRoot _resolver;

		internal NinjectDependencyScope(IResolutionRoot resolver)
		{
			Contract.Assert(resolver != null);

			this._resolver = resolver;
		}

		public void Dispose()
		{
			var disposable = this._resolver as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}

			this._resolver = null;
		}

		public object GetService(Type serviceType)
		{
			if (this._resolver == null)
			{
				throw new ObjectDisposedException("this", "This scope has already been disposed");
			}

			return this._resolver.TryGet(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			if (this._resolver == null)
			{
				throw new ObjectDisposedException("this", "This scope has already been disposed");
			}

			return this._resolver.GetAll(serviceType);
		}
	}

	public class NinjectDependencyResolver : System.Web.Mvc.IDependencyResolver //, System.Web.Http.Dependencies.IDependencyResolver
	{
		readonly IKernel _kernel;

		public NinjectDependencyResolver(IKernel kernel)
		{
			this._kernel = kernel;
		}

		public object GetService(Type serviceType)
		{
			return this._kernel.TryGet(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return this._kernel.GetAll(serviceType);
		}
	}
}
