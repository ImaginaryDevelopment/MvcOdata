using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Webby
{
	using System.Collections.Concurrent;
	using System.Reflection;
	using System.ServiceModel;

	public static class WcfServiceRegistry
	{
		private static readonly IDictionary<string, Type> TypeMap = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
		public static bool IsDataService(this Type type)
		{
			return type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.Name.StartsWith("WcfDataService");
		}
		public static void ScanAssemblies()
		{
			TypeMap.Clear();

			try
			{
				var assemblies = from a in AppDomain.CurrentDomain.GetAssemblies()
												 //where a.FullName.ToUpper().Contains("companyname") || a.FullName.ToUpper().Contains("companyname")
												 select a;

				// get all public types
				var types = (from a in assemblies from t in a.GetTypes() where t.IsPublic select t).ToList();

				// get all interfaces that have the ServiceContract attribtute
				var serviceContractInterfaces = (from t in types
																				 where t.IsInterface && t.GetCustomAttributes(typeof(ServiceContractAttribute), false).Any()
																				 select t).ToList();

				// get all types that implement the above interfaces
				var serviceContractImplementations = (from t in types
																							from i in serviceContractInterfaces
																							where t.IsClass && i.IsAssignableFrom(t)
																							select t).Distinct().ToList();

				var wcfDataServices = (from t in types.Where(x => x.IsDataService()) select t).Distinct().ToList();

				// NOTE: Could blow up if there are more than one implementing type with the same class name.
				serviceContractImplementations.ForEach(x => TypeMap.Add(x.Name, x));

				wcfDataServices.ForEach(x => TypeMap.Add(x.Name, x));
			}
			catch (ReflectionTypeLoadException e)
			{
				var message = string.Join(Environment.NewLine, e.LoaderExceptions.Select(ex => ex.ToString()));
				//Logger.Error(message, e);
				throw;
			}
		}

		public static void AddType(Type t)
		{
			TypeMap.Add(t.Name, t);
		}

		public static Type GetType(string serviceName)
		{
			Type t;
			TypeMap.TryGetValue(serviceName, out t);
			return t;
		}
	}
}
