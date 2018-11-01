using AssurBox.Samples.API.Insurance.Security;
using System.Web;
using System.Web.Mvc;

namespace AssurBox.Samples.API.Insurance
{
    public class FilterConfig
    {
        /// <summary>
        /// Filtres MVC
        /// </summary>
        /// <param name="filters"></param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        /// <summary>
        /// Filtres Web Api
        /// </summary>
        /// <param name="filters"></param>
        public static void RegisterWebApiFilters(System.Web.Http.Filters.HttpFilterCollection filters)
        {
            filters.Add(new InsuranceAuthorizeAttribute());
        }
    }
}
