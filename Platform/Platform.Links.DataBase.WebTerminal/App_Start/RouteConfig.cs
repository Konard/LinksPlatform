using System.Web.Mvc;
using System.Web.Routing;

namespace Platform.Links.DataBase.WebTerminal.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Links", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
