using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contracts
{
	using System.Xml;

	public interface IConfig
	{
		bool AppSettingExists(string key);

		string ReadAppSetting(string key);

		string TryReadAppSetting(string key);

		XmlNode ReadNode(string nodeToRead);

		string ReadValue(string configSectionName, string nodeOfInterest);

		string ReadValue(string section, string key, string defaultValue);
	}

	public class Config : IConfig
	{
		IDictionary<string, string> defaults = new Dictionary<string, string> { {"DefaultServiceRootUri","http://localhost/" },};
		public Config()
		{

		}
		public bool AppSettingExists(string key)
		{
			return defaults.ContainsKey(key);
		}

		public string ReadAppSetting(string key)
		{
			return defaults[key];
		}

		public string TryReadAppSetting(string key)
		{
			if (this.AppSettingExists(key)) return defaults[key];
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
