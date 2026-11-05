// CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
// MOD-09: consulta autorizada en mostrador. No reemplaza el flujo comercial del cliente.
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;

namespace AerolineaVIP_MVC.Controllers
{
    [Authorize(Roles = RolesAerolinea.PersonalCounter)]
    public class CounterController : Controller
    {
        [HttpGet]
        public ActionResult Index(string codigo)
        {
            var modelo = new CounterConsultaViewModel { CodigoBusqueda = (codigo ?? "").Trim() };
            if (string.IsNullOrWhiteSpace(modelo.CodigoBusqueda)) return View(modelo);
            if (modelo.CodigoBusqueda.Length > 40)
            {
                modelo.Mensaje = "Introduzca un código de reserva válido.";
                return View(modelo);
            }

            using (var db = new AerolineaVIPEntities())
            {
                // Consultar por código exacto evita mostrar listados de clientes no solicitados.
                var reserva = db.Reservas.AsNoTracking()
                    .FirstOrDefault(r => r.CodigoReserva == modelo.CodigoBusqueda);
                if (reserva == null)
                {
                    modelo.Mensaje = "No existe una reserva con el código indicado.";
                    return View(modelo);
                }

                modelo.CodigoReserva = reserva.CodigoReserva;
                modelo.EstadoReserva = reserva.EstadoReservaCodigo;
                modelo.FechaReserva = reserva.FechaCreacion;
                modelo.Pasajeros = db.ReservaPasajeroVueloes.AsNoTracking()
                    .Include(a => a.Pasajero)
                    .Include(a => a.Vuelo)
                    .Where(a => a.ReservaId == reserva.ReservaId)
                    .OrderBy(a => a.AsignacionId)
                    .ToList()
                    .Select(a => new CounterPasajeroFila
                    {
                        Nombre = a.Pasajero.Nombres + " " + a.Pasajero.Apellidos,
                        VueloId = a.VueloId,
                        FechaVuelo = a.Vuelo.FechaOperacion,
                        Asiento = a.NumeroAsiento,
                        Estado = a.EstadoAsignacionCodigo
                    }).ToList();
                return View(modelo);
            }
        }
    }
}
