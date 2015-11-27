using System.Web.Http;
using System.Web.Http.OData.Extensions;
using Owin;

namespace AutoMapperSamples.Breeze
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "BreezeApi",
                routeTemplate: "breeze/{controller}/{action}"
                );
            
            appBuilder.UseWebApi(config);
        }
    }
}
