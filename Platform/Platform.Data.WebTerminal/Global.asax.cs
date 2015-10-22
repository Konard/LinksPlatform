using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Platform.Data.Core.Triplets;
using Platform.Data.WebTerminal.App_Start;

namespace Platform.Data.WebTerminal
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var databaseFile = Path.Combine(Server.MapPath("App_Data"), @"data.dat");

            File.Delete(databaseFile);

            Link.StartMemoryManager(databaseFile);
        }

        public void Application_End()
        {
            Link.StopMemoryManager();
        }
    }
}
