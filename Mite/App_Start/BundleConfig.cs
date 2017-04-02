using System.Web;
using System.Web.Optimization;

namespace Mite
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/main").Include(
                "~/Scripts/jquery-3.1.1.js",
                "~/Scripts/semantic.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/jquery.address.js",
                "~/bower_components/perfect-scrollbar/js/perfect-scrollbar.jquery.js",
                "~/Scripts/ViewHelpers.js",
                "~/Scripts/jquery.signalR-2.2.1.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/croppie").Include("~/node_modules/croppie/croppie.js"));
            bundles.Add(new ScriptBundle("~/bundles/editor").Include(
                "~/Scripts/content-tools.js"));
            bundles.Add(new ScriptBundle("~/bundles/jsrender").Include("~/bower_components/jsrender/jsrender.min.js"));

            bundles.Add(new StyleBundle("~/Content/semantic").Include(
                "~/bower_components/semantic/src/semantic.css"
                ));
            bundles.Add(new StyleBundle("~/Content/landing").Include(
                "~/Content/landing.css"
                ));
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/node_modules/croppie/croppie.css",
                "~/Content/Site.css",
                "~/Content/ident.css",
                "~/Content/content-tools.min.css",
                "~/bower_components/perfect-scrollbar/css/perfect-scrollbar.css"));
        }
    }
}
