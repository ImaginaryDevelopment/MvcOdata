namespace Contracts
{
	using System.Collections.Generic;
	using System.Xml;

	public class Config : IConfig
	{
		readonly IDictionary<string, string> defaults = new Dictionary<string, string> { { "DefaultServiceRootUri", "http://localhost:4339/{0}.svc" }, };

		public bool AppSettingExists(string key)
		{
			return this.defaults.ContainsKey(key);
		}

		public string ReadAppSetting(string key)
		{
			return this.defaults[key];
		}

		public string TryReadAppSetting(string key)
		{
			if (this.AppSettingExists(key)) return this.defaults[key];
			return null;
		}

		public XmlNode ReadNode(string nodeToRead)
		{
			return new XmlDocument().FirstChild;
		}

		public string ReadValue(string configSectionName, string nodeOfInterest)
		{
			return string.Empty;
		}

		public string ReadValue(string section, string key, string defaultValue)
		{
			return string.Empty;
		}
	}
}