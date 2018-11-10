using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AssurBox.Samples.Client.Garage.Web.Controllers
{
    public class AssurBoxNotificationController : Controller
    {
        
        [Route("assurboxwebhook")]
        public HttpStatusCodeResult AssurBoxWebHook(AssurBox.SDK.WebHooks.GreenCardRequestNotification notification)
        {
            //notification.CorrelationId -- correlationid is the identifier for the whole request file
            //notification.NotificationType --> type will be Response for a car dealer



            return new HttpStatusCodeResult(HttpStatusCode.OK); // should answer with a HTTP 200
        }
    }
}