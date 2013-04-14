using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webby
{
	using System.Data.Services;

	using Contracts;

	public abstract class WcfDataService<T> : DataService<T>
	{
		
		// Left this out as there isn't an obvious way to implement it at the moment. I could have missed something though.
		// private readonly bool _logResponses = bool.Parse(Config.ReadAppSetting("LogWcfResponses", "false"));
		//private readonly bool _logRequests = bool.Parse(new Config().ReadAppSetting("LogWcfRequests", "false"));

		protected override void OnStartProcessingRequest(ProcessRequestArgs args)
		{
		
			//if (this._logRequests)
			//{
			//	LogRequest(args);
			//}

			base.OnStartProcessingRequest(args);
		}

		private static void LogRequest(ProcessRequestArgs args)
		{
			// ReSharper disable EmptyGeneralCatchClause
			try
			{
				var sb = new StringBuilder();
				sb.AppendLine("WCF Data Service Request");
				sb.AppendFormat("Operation: {0}{1}", args.RequestUri, Environment.NewLine);
				sb.AppendFormat("RequestMethod: {0}{1}", args.OperationContext.RequestMethod, Environment.NewLine);
				foreach (var k in args.OperationContext.RequestHeaders.AllKeys)
				{
					sb.AppendFormat("Header: '{0}' Value: '{1}'{2}", k, args.OperationContext.RequestHeaders[k], Environment.NewLine);
				}

				//Logger.Info(sb.ToString());
			}
			catch (Exception)
			{
				// Nom nom nom
			}

			// ReSharper restore EmptyGeneralCatchClause
		}

	}
}
