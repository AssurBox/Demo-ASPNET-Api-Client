using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire.Annotations;

namespace AssurBox.Samples.API.Insurance
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            // Obviously you should protect the dashboard, this is for demo purpose
            return true;
        }
    }
}