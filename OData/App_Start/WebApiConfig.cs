﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace OData
{
	using System.Web.Http.OData.Builder;
	using System.Web.Routing;

	using OdataGenerator;

	public static class WebApiConfig
	{
		public const string DefaultODataRoute = "DefaultOData";

		public static void Register(HttpConfiguration config)
		{

			var modelBuilder = new ODataConventionModelBuilder();
			ODataConventionModelBuilderEnabler.WireModels(modelBuilder);

			var model = modelBuilder.GetEdmModel();
			config.Routes.MapODataRoute("OData", "OData", model);
			var odataRoute = RouteTable.Routes["OData"] as Route;
            
			config.Routes.MapHttpRoute(DefaultODataRoute, "OData/{controller}/{id}", new { id = RouteParameter.Optional });
			config.Routes.MapHttpRoute(
					name: "DefaultApi",
					routeTemplate: "api/{controller}/{id}",
					defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}