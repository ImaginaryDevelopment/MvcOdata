using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfData
{
	using System.Web.Hosting;

	using Webby;

	public class AppStart
	{
		public static void AppInitialize()
		{
			var provider = new WcfVirtualPathProvider();
			HostingEnvironment.RegisterVirtualPathProvider(provider);
			WcfServiceRegistry.ScanAssemblies();
			System.Diagnostics.Debugger.Launch();
		}
	}
}