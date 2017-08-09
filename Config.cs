using System;
using System.Configuration;

namespace MailImporter
{
    public static class Config
    {
        public static string GetValue(string keyName, string defaultValue)
        {
            string result = ConfigurationManager.AppSettings[keyName];
            if (result != null)
            {
                if (result.Length == 0)
                {
                    Logger.Log($"Configuration key {keyName} is empty! Using {defaultValue} instead", LogType.WARNING);
                }
                else
                {
                    return result;
                }
            }
            else
            {
                Logger.Log($"Configuration key {keyName} not found! Using {defaultValue} instead", LogType.WARNING);
            }
            return defaultValue;
        }

        public static T GetValue<T>(string keyName, T defaultValue) where T : IConvertible
        {
            string stringResult = GetValue(keyName, defaultValue.ToString());
            try
            {
                return (T)Convert.ChangeType(stringResult, typeof(T));
            }
            catch(Exception ex)
            {
                Logger.Log($"{keyName} value ({stringResult}) cannot be converted to type {typeof(T).Name}: {ex.Message}. Using {defaultValue} instead", LogType.WARNING);
                return defaultValue;
            }
        }
    }
}
