using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Platform.Links.DataBase.CoreNet.Triplets;
using Platform.Links.DataBase.WebTerminal.App_Start;

namespace Platform.Links.DataBase.WebTerminal
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

            Link.StartMemoryManager(Path.Combine(Server.MapPath("App_Data"), @"data.dat"));
        }

        public void Application_End()
        {
            Link.StopMemoryManager();
        }
    }
}
