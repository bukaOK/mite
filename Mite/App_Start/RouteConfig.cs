using System.Web.Mvc;
using System.Web.Routing;

namespace Mite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("Public/{all}");
            routes.LowercaseUrls = true;

            routes.MapMvcAttributeRoutes();

            routes.MapRoute("Sitemap", "sitemap.xml", new { controller = "Sitemap", action = "Index" });
            routes.MapRoute(
                name: "UserProfile",
                url: "User/Profile/{name}/{action}/{type}",
                defaults: new { controller = "UserProfile", action = "Index", type = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "UserSettings",
                url: "User/Settings/{action}",
                defaults: new {controller = "UserSettings", action = "Index"}
            );
            routes.MapRoute(
                name: "UserNotifications",
                url: "user/notifications",
                defaults: new { controller = "UserProfile", action = "Notifications" }
            );
            routes.MapRoute(
                name: "AuthorServices", 
                url: "author/services/{action}/{id}",
                defaults: new { controller = "AuthorServices", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "UserDeals",
                url: "user/deals/{action}/{type}",
                defaults: new { controller = "UserDeals", Action = "Index", type = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "NotFound",
                url: "{all}",
                defaults: new { controller = "Error", action = "NotFound", id = UrlParameter.Optional }
            );
        }
    }
}
