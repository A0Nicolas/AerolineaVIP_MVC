using System.Web.Mvc;

namespace AerolineaVIP_MVC.Controllers
{
    public class HomeController : Controller
    {
        // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
        // Primera entrega, fase 1: portada y estructura navegable; la busqueda se implementara despues.
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
    }
}
