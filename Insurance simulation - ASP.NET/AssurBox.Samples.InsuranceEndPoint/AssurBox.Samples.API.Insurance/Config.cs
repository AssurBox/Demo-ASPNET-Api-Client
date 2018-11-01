using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

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
    }
}