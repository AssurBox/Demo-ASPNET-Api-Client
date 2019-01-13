using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AssurBox.Samples.Client.Garage.Web
{
    public class Config
    {
  

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

        public static string GetAssurBoxSDKVersion()
        {
            return Assembly.GetAssembly(typeof(AssurBox.SDK.ApiError)).GetName().Version.ToString();

        }
    }
}