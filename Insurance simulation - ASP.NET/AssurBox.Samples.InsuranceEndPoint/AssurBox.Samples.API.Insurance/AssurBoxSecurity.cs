using AssurBox.SDK;
using AssurBox.SDK.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace AssurBox.Samples.API.Insurance
{
    public class AssurBoxSecurity
    {

        /// <summary>
        /// Gets the JWT token to communicate with AssurBox
        /// </summary>
        /// <returns></returns>
        public static string GetAssurBoxSecurityToken()
        {

            string token = MemoryCache.Default["ABX_TOKEN"] as string;
            if (string.IsNullOrEmpty(token))
            {
                SecurityClient id = new SecurityClient(new AssurBoxClientOptions { Host = Config.AssurBoxApiBaseURL });
                var tokeninfo = id.GetBearerToken(Config.AssurBoxApiClientID, Config.AssurBoxApiClientSecret).Result;
                token = tokeninfo.access_token;
                MemoryCache.Default["ABX_TOKEN"] = token;
            }
            return token;

        }
    }
}