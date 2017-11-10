using System.Web;
using System.Web.Optimization;

namespace Mite
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //JS
            bundles.Add(new ScriptBundle("~/bundles/jquery", "https://code.jquery.com/jquery-3.2.1.min.js").Include(
                "~/Scripts/jquery-3.1.1.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/semantic", "https://cdnjs.cloudflare.com/ajax/libs/semantic-ui/2.2.13/semantic.min.js")
                .Include("~/bower_components/semantic/dist/semantic.js"));

            bundles.Add(new ScriptBundle("~/bundles/site")
                .Include("~/Scripts/jquery.address.js",
                "~/Scripts/perfect-scrollbar.js",
                "~/bower_components/simplebar/simplebar.js",
                "~/bower_components/izitoast/dist/js/iziToast.min.js",
                "~/Scripts/jquery.signalR-2.2.2.min.js")
                .IncludeDirectory("~/Scripts/Custom/", "*.js"));
            bundles.Add(new ScriptBundle("~/bundles/inputmask").Include("~/Scripts/jquery.maskedinput.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/api")
                .IncludeDirectory("~/Scripts/Api/", "*.js")
                .IncludeDirectory("~/Scripts/Mvc", "*.js"));

            bundles.Add(new ScriptBundle("~/bundles/croppie").Include("~/node_modules/croppie/croppie.js"));
            bundles.Add(new ScriptBundle("~/bundles/editor").Include(
                "~/Scripts/content-tools.js", "~/bower_components/pep/src/jquery.pep.js"));
            bundles.Add(new ScriptBundle("~/bundles/jsrender").Include("~/bower_components/jsrender/jsrender.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/grid").Include("~/Scripts/masonry.pkgd.min.js", 
                "~/bower_components/imagesloaded/imagesloaded.pkgd.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/clipboard").Include("~/bower_components/clipboard/dist/clipboard.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/scroll").Include(
                "~/bower_components/jquery.scrollTo/jquery.scrollTo.js",
                "~/bower_components/jquery.easing/js/jquery.easing.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/gallery").Include(
                "~/bower_components/lightgallery/lib/jquery.mousewheel.min.js",
                "~/Content/gallery/js/lightgallery.min.js",
                "~/bower_components/lightgallery/modules/lg-fullscreen.min.js",
                "~/bower_components/lightgallery/modules/lg-pager.min.js",
                "~/bower_components/lightgallery/modules/lg-zoom.min.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/datepicker").Include(
                "~/bower_components/air-datepicker/dist/js/datepicker.min.js"));

            //CSS
            bundles.Add(new StyleBundle("~/Content/semantic").Include(
                "~/bower_components/semantic/src/semantic.css"
            ));
            bundles.Add(new StyleBundle("~/Content/landing").Include(
                "~/Content/landing.css"
            ));
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/node_modules/croppie/croppie.css",
                "~/bower_components/izitoast/dist/css/iziToast.min.css",
                "~/Content/Site.css",
                "~/Content/ident.css",
                "~/Content/content-tools.min.css",
                "~/bower_components/simplebar/simplebar.css",
                "~/node_modules/perfect-scrollbar/css/perfect-scrollbar.css",
                "~/Content/UserMenu.css"
            ));
            bundles.Add(new StyleBundle("~/Content/datepicker").Include(
                "~/bower_components/air-datepicker/dist/css/datepicker.css"));
            bundles.Add(new StyleBundle("~/Content/gallery").Include(
                "~/Content/gallery/css/lightgallery.min.css",
                "~/Content/gallery.css"
            ));
            bundles.Add(new StyleBundle("~/Content/dialog").Include("~/Content/dialogs.css"));
            //LESS
            bundles.Add(new LessBundle("~/Content/less").Include(
                "~/Content/DotLoader.less"
            ));
        }
    }
}
