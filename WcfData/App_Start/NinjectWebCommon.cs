[assembly: WebActivator.PreApplicationStartMethod(typeof(WcfData.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(WcfData.App_Start.NinjectWebCommon), "Stop")]

namespace WcfData.App_Start
{
	using System;
	using System.Web;

	using Contracts;

	using Microsoft.Practices.ServiceLocation;
	using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

	using Webby;

	public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
					kernel.Bind(typeof(Webby.IWcfProxyFactory<>)).To(typeof(Webby.WcfProxyFactory<>));
	        kernel.Bind<IStarfleetCommander>().To<StarfleetCommandMemoryRepository>(); // for EF use StarfleetCommandRepository
	        //kernel.Bind<Func<MaslowJax_dbsEntities>>().ToMethod(ctx => () => new MaslowJax_dbsEntities("DefaultConnection"));
	        ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));
        }        
    }
}
