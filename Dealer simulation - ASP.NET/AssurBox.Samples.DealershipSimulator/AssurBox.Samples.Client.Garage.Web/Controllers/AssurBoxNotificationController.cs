using Microsoft.AspNet.SignalR;
using System.Net;
using System.Web.Mvc;

namespace AssurBox.Samples.Client.Garage.Web.Controllers
{
    public class AssurBoxNotificationController : Controller
    {
        /// <summary>
        /// This is a webhook, if configured in AssurBox, REST notifications will be sent to this endpoint
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        [Route("assurboxwebhook")]
        [HttpPost]
        public HttpStatusCodeResult AssurBoxWebHook(AssurBox.SDK.WebHooks.GreenCardRequestNotification notification)
        {
            // do someting with the notification
            //notification.CorrelationId -- correlationid is the identifier for the whole request file
            //notification.NotificationType --> type will be Response for a car dealer


            var clients = GlobalHost.ConnectionManager.GetHubContext<Hubs.NotificationHub>().Clients;
            clients.All.broadcastMessage("Notification from AssurBox for Licence plate " + notification.LicencePlate, notification.Communication);

            return new HttpStatusCodeResult(HttpStatusCode.OK); // should answer to AssurBox with a HTTP 200
        }
    }
}