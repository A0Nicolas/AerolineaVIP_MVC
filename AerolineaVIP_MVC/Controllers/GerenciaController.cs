// CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
// Consulta agregada para Gerente: sin acceso a roles, datos personales ni cambios CRUD.
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;

namespace AerolineaVIP_MVC.Controllers
{
    [Authorize(Roles = RolesAerolinea.ConsultaGerencia)]
    public class GerenciaController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new AerolineaVIPEntities())
            {
                var modelo = new GerenciaResumenViewModel
                {
                    Vuelos = db.Vueloes.Count(),
                    Aeronaves = db.Aeronaves.Count(),
                    Reservas = db.Reservas.Count(),
                    Compras = db.Compras.Count()
                };
                return View(modelo);
            }
        }
    }
}
