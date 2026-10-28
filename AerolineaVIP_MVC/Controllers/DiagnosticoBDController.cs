using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;

namespace AerolineaVIP_MVC.Controllers
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Fase 3, Nico: diagnóstico TEMPORAL de lectura de la base de dominio.
    // Utiliza el contexto creado por Visual Studio desde Model1.edmx (Database First).
    // No reemplaza los CRUD por scaffolding ni forma parte de las 21 pantallas finales.
    [Authorize]
    public class DiagnosticoBDController : Controller
    {
        // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
        // Consulta de solo lectura para comprobar EF6 + SQL Server 2019.
        public ActionResult Index()
        {
            var modelo = new DiagnosticoBDViewModel();

            try
            {
                using (var db = new AerolineaVIPEntities())
                {
                    modelo.TotalAeronaves = db.Aeronaves.Count();
                    modelo.TotalVuelos = db.Vueloes.Count();

                    modelo.Aeronaves = db.Aeronaves
                        .OrderBy(a => a.AeronaveId)
                        .Select(a => new AeronaveResumen
                        {
                            AeronaveId = a.AeronaveId,
                            Matricula = a.Matricula,
                            Estado = a.EstadoAeronaveCodigo
                        })
                        .ToList();

                    modelo.Vuelos = db.Vueloes
                        .OrderBy(v => v.FechaOperacion)
                        .ThenBy(v => v.VueloId)
                        .Take(10)
                        .Select(v => new VueloResumen
                        {
                            VueloId = v.VueloId,
                            FechaOperacion = v.FechaOperacion,
                            AeronaveId = v.AeronaveId,
                            Estado = v.EstadoVueloCodigo
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
                // No se expone la cadena de conexión ni información sensible al navegador.
                Trace.TraceError("Error en DiagnosticoBD: " + ex);
                modelo.Error = "No se pudo consultar AerolineaVIP. Revisa la conexión del EDMX y los permisos SQL.";
            }

            return View(modelo);
        }
    }
}
