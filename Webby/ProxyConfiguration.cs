namespace Webby
{
	using System;
	using System.ServiceModel;

	using Contracts;

	public class ProxyConfiguration
	{
		private readonly IConfig _config;

		private const string ConfigKeyFormat = "{0}.{1}";

		public ProxyConfiguration(IConfig config, string serviceName, string instance = null)
		{
			this._config = config;
			this.EndpointConfigurationName = serviceName;
			this.Instance = instance;
			this.UseEndpointConfiguration = this.GetBoolValue("UseEndpointConfiguration", false);
			this.Address = this.GetEndpointAddress();
			this.BindingType = this.GetBindingType();
			this.ServiceTypeName = this.GetServiceTypeName();
			this.OperationTimeout = this.GetTimespanValue("OperationTimeout");
			this.MaxReceivedMessageSize = this.GetIntValue("MaxReceivedMessageSize");
			this.ReceiveTimeout = this.GetTimespanValue("ReceiveTimeout");
			this.SendTimeout = this.GetTimespanValue("SendTimeout");
			this.OpenTimeout = this.GetTimespanValue("OpenTimeout");
			this.MaxItemsInObjectGraph = this.GetIntValue("MaxItemsInObjectGraph");
			this.MaxArrayLength = this.GetIntValue("MaxArrayLength", 16384);
			this.MaxBytesPerRead = this.GetIntValue("MaxBytesPerRead", 4096);
			this.MaxDepth = this.GetIntValue("MaxDepth", 32);
			this.MaxNameTableCharCount = this.GetIntValue("MaxNameTableCharCount", 16384);
			this.MaxStringContentLength = this.GetIntValue("MaxStringContentLength", 8192);
			this.MessageEncoding = this.GetValue("MessageEncoding") == null
				                  ? WSMessageEncoding.Mtom
				                  : (WSMessageEncoding)Enum.Parse(typeof(WSMessageEncoding), this.GetValue("MessageEncoding"), true);
			this.IncludeExceptionDetailInFaults = this.GetBoolValue("IncludeExceptionDetailInFaults", true);
		}

		public IConfig Config { get; protected set; }

		public string EndpointConfigurationName { get; protected set; }

		public string Instance { get; protected set; }

		public bool UseEndpointConfiguration { get; protected set; }

		public Type BindingType { get; protected set; }

		public string ServiceTypeName { get; protected set; }

		public EndpointAddress Address { get; protected set; }

		public int MaxItemsInObjectGraph { get; protected set; }

		public TimeSpan OperationTimeout { get; protected set; }

		public int MaxReceivedMessageSize { get; protected set; }

		public TimeSpan ReceiveTimeout { get; protected set; }

		public TimeSpan SendTimeout { get; protected set; }

		public TimeSpan OpenTimeout { get; protected set; }

		public int MaxArrayLength { get; protected set; }

		public int MaxBytesPerRead { get; protected set; }

		public int MaxDepth { get; protected set; }

		public int MaxNameTableCharCount { get; protected set; }

		public int MaxStringContentLength { get; protected set; }

		public WSMessageEncoding MessageEncoding { get; protected set; }

		public bool IncludeExceptionDetailInFaults { get; protected set; }

		private string GetValue(string key)
		{
			if (!string.IsNullOrEmpty(this.Instance))
			{
				return this._config.ReadAppSettingOrNull(string.Format("{0}.{1}.{2}", this.EndpointConfigurationName, this.Instance, key));
			}

			var configKey = string.Format(ConfigKeyFormat, this.EndpointConfigurationName, key);

			return this._config.ReadAppSettingOrNull(configKey) ?? this._config.ReadAppSettingOrNull(string.Format("{0}.{1}", this.EndpointConfigurationName, key));
		}

		private bool GetBoolValue(string key, bool defaultValue)
		{
			var value = this.GetValue(key);

			return value == null ? defaultValue : Convert.ToBoolean(value);
		}

		private int GetIntValue(string key, int defaultValue = Int32.MaxValue)
		{
			var value = this.GetValue(key);

			return value == null ? defaultValue : Convert.ToInt32(value);
		}

		private TimeSpan GetTimespanValue(string key)
		{
			var value = this.GetValue(key);

			return value == null ? new TimeSpan(0, 0, 0, 0, Int32.MaxValue) : new TimeSpan(0, 0, 0, 0, Convert.ToInt32(value));
		}

		private EndpointAddress GetEndpointAddress()
		{
			var uri = this.GetValue("Uri");

			if (!string.IsNullOrEmpty(this.Instance) && string.IsNullOrEmpty(uri))
			{
				return null;
			}

			var defaultRoot = this._config.ReadAppSettingOrNull("DefaultServiceRootUri");

			var serviceUri = string.IsNullOrWhiteSpace(uri) ? new Uri(string.Format(defaultRoot, this.EndpointConfigurationName)) : new Uri(uri);

			var serviceUpn = this.GetValue("ServiceUpn") ?? this._config.ReadAppSettingOrNull("ServiceUpn");

			return !string.IsNullOrEmpty(serviceUpn)
				       ? new EndpointAddress(serviceUri, EndpointIdentity.CreateUpnIdentity(serviceUpn))
				       : new EndpointAddress(serviceUri);
		}

		private Type GetBindingType()
		{
			var typestring = this.GetValue("BindingType") ?? this._config.ReadAppSettingOrNull("BindingType");

			return typestring == null ? typeof(WSHttpBinding) : Type.GetType(typestring, true, true);
		}

		private string GetServiceTypeName()
		{
			var typeString = this.GetValue("ServiceType");

			if (typeString != null)
			{
				return typeString;
			}

			var type = WcfServiceRegistry.GetType(this.EndpointConfigurationName);

			return type == null ? string.Format("{0}.{0}, {0}", this.EndpointConfigurationName) : type.AssemblyQualifiedName;
		}

		public static ProxyConfiguration Create(IConfig config, string serviceName, string instance = null)
		{
			var proxyConfig = new ProxyConfiguration(config, serviceName, instance);

			return proxyConfig.Address == null ? null : proxyConfig;
		}
	}
}