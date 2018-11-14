using Microsoft.Owin;
using Owin;

/*
 SignalR tutorial: https://docs.microsoft.com/en-us/aspnet/signalr/overview/getting-started/tutorial-getting-started-with-signalr-and-mvc
 */

[assembly: OwinStartup(typeof(AssurBox.Samples.Client.Garage.Web.Startup))]

namespace AssurBox.Samples.Client.Garage.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}