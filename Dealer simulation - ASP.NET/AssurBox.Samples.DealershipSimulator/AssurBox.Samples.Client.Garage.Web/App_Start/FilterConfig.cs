using System.Web;
using System.Web.Mvc;

namespace AssurBox.Samples.Client.Garage.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
