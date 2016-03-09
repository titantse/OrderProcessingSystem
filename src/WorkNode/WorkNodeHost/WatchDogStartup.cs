

namespace OrderProcessing.WorkNode
{
    using System.Web.Http;
    using Owin;

    public class WatchDogStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute("Watchdog_Selfclosure", "WatchDog/Selfclosure",
                new {controller = "WatchDog", action = "Selfclosure"}
                );
            config.Routes.MapHttpRoute("Watchdog_External", "WatchDog/External",
                new {controller = "WatchDog", action = "External"}
                );

            config.Routes.MapHttpRoute("WorkNode_Stop", "Command/Stop",
                new {controller = "Command", action="Stop"}
                );
            config.Routes.MapHttpRoute("WorkNode_Stat", "Command/Stat",
                new { controller = "Command", action = "Stat" }
                );

            appBuilder.UseWebApi(config);
        }
    }
}