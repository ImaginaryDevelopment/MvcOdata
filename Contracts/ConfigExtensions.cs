namespace Contracts
{
	using System.ComponentModel;

	public static class ConfigExtensions
	{
		public static string ReadAppSettingOrNull(this IConfig config, string key)
		{
			if (config.AppSettingExists(key))
			{
				return config.ReadAppSetting(key);
			}

			return null;
		}

		public static T ReadAppSettingOrDefault<T>(this IConfig config, string key, T defaultValue = default(T))
		{
			if (!config.AppSettingExists(key))
			{
				return defaultValue;
			}

			return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(config.ReadAppSetting(key));
		}

		public static T ReadValueOrDefault<T>(this IConfig config, string sectionToRead, string key, T defaultValue = default(T))
		{
			var stringValue = config.ReadValue(sectionToRead, key, null);

			if (stringValue == null)
			{
				return defaultValue;
			}

			return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(stringValue);
		}
	}
}