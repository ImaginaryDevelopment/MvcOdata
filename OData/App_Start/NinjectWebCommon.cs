[assembly: WebActivator.PreApplicationStartMethod(typeof(OData.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(OData.App_Start.NinjectWebCommon), "Stop")]

namespace OData.App_Start
{
    using System;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;

    using Contracts;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    using Webby;

    using NinjectDependencyResolver = Ninject.Web.Mvc.NinjectDependencyResolver;

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
						GlobalConfiguration.Configuration.DependencyResolver = new NinjectApiResolver(kernel);
						DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
					var config=new Config();
					kernel.Bind(typeof(IWcfProxyFactory<>)).To(typeof(WcfProxyFactory<>));
					kernel.Bind(typeof(IDataServiceContextFactory<>))
								 .To(typeof(DataServiceContextFactory<>))
								 .WithConstructorArgument("uri", new Uri("http://localhost:4339/starfleetcommander.svc"));
					kernel.Bind<IStarfleetCommander>().ToMethod(c => c.Kernel.Get<IDataServiceContextFactory<IStarfleetCommander>>().GetContext(config));
        }        
    }
}
