using AssurBox.Samples.Client.Garage.Web.Core;
using AssurBox.Samples.Client.Garage.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AssurBox.Samples.Client.Garage.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            // Dev data : demo account
            CredentialsModel model = new CredentialsModel
            {
                ClientID = "Mk8znT1xbFTWghTx82vc2g==",
                ClientSecret = "NTjH6WtvYaVmBCpx9+9VGj7hGoDFCpIIBehPBg7K604YQFzIPoxr+TbST+R2qI/GAgVzayfYoytNbv6EO61sfQ=="
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(CredentialsModel model)
        {
            // test f
            using (SDK.Clients.IdentityClient client = new SDK.Clients.IdentityClient(new SDK.AssurBoxClientOptions ()))
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