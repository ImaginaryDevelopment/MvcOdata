using System;
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
}
