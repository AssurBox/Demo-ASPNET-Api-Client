using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssurBox.Samples.Client.Garage.Web.Core
{
    public static class SessionManager
    {
        public static string BearerToken
        {
            get
            {
                return HttpContext.Current.Session["_BEARERTOKEN_"] as string;
            }
            set
            {
                HttpContext.Current.Session["_BEARERTOKEN_"] = value;
            }
        }

        public static string UserName
        {
            get
            {
                return HttpContext.Current.Session["_USERNAME_"] as string;
            }
            set
            {
                HttpContext.Current.Session["_USERNAME_"] = value;
            }
        }
    }
}