using System.Web;
using System.Web.Optimization;

namespace AerolineaVIP_MVC
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información sobre los formularios.  De esta manera estará
            // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
            // Adaptación al mecanismo de bundles de la PRUEBA03_MVC funcional.
            // Bootstrap se carga con Bundle para evitar minificación de WebGrease
            // incompatible con algunos archivos JavaScript modernos.
            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
            // Se incluye hoja de estilo temporal de la Aerolínea VIP.
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css",
                      "~/Content/vip.css"));
        }
    }
}
