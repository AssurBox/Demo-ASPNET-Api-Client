using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace AssurBox.Samples.API.Insurance
{
    public class Config
    {
        public static string AccessKey
        {
            get
            {
                return ConfigurationManager.AppSettings["AssurBox:Insurance:Api:AccessKey"];
            }
        }

        public static string AssurBoxApiBaseURL
        {
            get
            {
                return ConfigurationManager.AppSettings["AssurBox:Api:BaseURL"];
            }
        }

        public static string AssurBoxApiClientID
        {
            get
            {
                return ConfigurationManager.AppSettings["AssurBox:Api:ClientID"];
            }
        }

        public static string AssurBoxApiClientSecret
        {
            get
            {
                return ConfigurationManager.AppSettings["AssurBox:Api:ClientSecret"];
            }
        }

        public static DateTime GetBuildDate()
        {
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            return File.GetLastAccessTimeUtc(
                Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path))
                );

        }
    }
}