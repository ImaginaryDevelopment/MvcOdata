using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Caching;
using System.Web.Hosting;

namespace Webby
{
	using Contracts;

	// This is the virtual .svc file.
	// When this file is accessed, a <%@ServiceHost%> tag is returned that gives
	// the service and factory associated with this VirtualFile.

	// This is the factory loaded when the user tried to use the (virtual) .svc
	// file. The constructorString is the "Service=" part of the .svc and for our
	// purposes identifies the name of the service class to be
	// loaded by reflection.

	// This is the VirtualPathProvider that serves up virtual files. The supported
	// requests are of the form "~/Service/ClassName.svc".
	public class WcfVirtualPathProvider : VirtualPathProvider
	{
		private static readonly object Lock = new object();

		private static readonly IDictionary<string, WcfVirtualFile> WcfVirtualFileCache = new Dictionary<string, WcfVirtualFile>(StringComparer.InvariantCultureIgnoreCase);

		public override bool FileExists(string virtualPath)
		{
			return IsVirtualFile(virtualPath) || Previous.FileExists(virtualPath);
		}

		public override VirtualFile GetFile(string virtualPath)
		{
			var isVirtualFile = IsVirtualFile(virtualPath);
			return isVirtualFile ? GetVirtualFile(virtualPath) : Previous.GetFile(virtualPath);
		}

		private static VirtualFile GetVirtualFile(string virtualPath)
		{
			WcfVirtualFile vf;

			if (WcfVirtualFileCache.TryGetValue(virtualPath, out vf))
			{
				return vf;
			}

			lock (Lock)
			{
				if (!WcfVirtualFileCache.ContainsKey(virtualPath))
				{
					var serviceName = Path.GetFileNameWithoutExtension(virtualPath);
					var pc = new ProxyConfiguration(new Config(), serviceName);
					vf = new WcfVirtualFile(virtualPath, pc.ServiceTypeName, typeof(VirtualWcfFactory).FullName);

					WcfVirtualFileCache.Add(virtualPath, vf);

					return vf;
				}
				return WcfVirtualFileCache[virtualPath];
			}
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			if (IsVirtualFile(virtualPath) || IsVirtualDirectory(virtualPath))
			{
				return null;
			}

			return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
		}

		private bool IsVirtualFile(string appRelativeVirtualPath)
		{
			return !Previous.FileExists(appRelativeVirtualPath) && appRelativeVirtualPath.EndsWith(".svc", StringComparison.InvariantCultureIgnoreCase);
		}

		private bool IsVirtualDirectory(string appRelativeVirtualPath)
		{
			return !Previous.DirectoryExists(appRelativeVirtualPath);
		}
	}
}
