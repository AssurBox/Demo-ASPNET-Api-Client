using AssurBox.Samples.Client.Garage.Web.Core;
using AssurBox.Samples.Client.Garage.Web.Models;
using AssurBox.SDK;
using AssurBox.SDK.Clients;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AssurBox.Samples.Client.Garage.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            CredentialsModel model = new CredentialsModel
            {
                ClientID = Config.AssurBoxApiClientID,
                ClientSecret = Config.AssurBoxApiClientSecret
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(CredentialsModel model)//
        {
            
            // AssurBoxEnvironments.Test targets https://sandbox.assurbox.net
            using (SecurityClient client = new SecurityClient(new AssurBoxClientOptions (AssurBoxEnvironments.Test)))
            {
                var token = await client.GetBearerToken(model.ClientID, model.ClientSecret);
                SessionManager.BearerToken = token.access_token;
                SessionManager.UserName = token.userName;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About this demo.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact us for any question or request.";

            return View();
        }
    }
}