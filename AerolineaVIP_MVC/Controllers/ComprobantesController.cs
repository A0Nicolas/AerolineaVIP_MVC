using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;
using Microsoft.AspNet.Identity;

namespace AerolineaVIP_MVC.Controllers
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Comprobantes de la operación académica; cada cliente solo ve los propios.
    [Authorize]
    public class ComprobantesController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            string usuarioId = User.Identity.GetUserId();
            using (var db = new AerolineaVIPEntities())
            {
                var datos = db.Comprobantes.Include(c => c.Compra.Reserva)
                    .Where(c => c.Compra.Reserva.UsuarioId == usuarioId)
                    .OrderByDescending(c => c.FechaEmision).Take(100).ToList();
                return View(datos.Select(c => new ComprobanteFilaViewModel {
                    ComprobanteId = c.ComprobanteId,
                    Numero = c.NumeroComprobante,
                    CodigoReserva = c.Compra.Reserva.CodigoReserva,
                    Fecha = c.FechaEmision, Importe = c.Compra.Importe,
                    Moneda = c.Compra.MonedaCodigo,
                    Estado = c.EstadoComprobanteCodigo,
                    Tipo = c.TipoComprobanteCodigo
                }).ToList());
            }
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            string usuarioId = User.Identity.GetUserId();
            using (var db = new AerolineaVIPEntities())
            {
                var c = db.Comprobantes.Include(x => x.Compra.Reserva)
                    .SingleOrDefault(x => x.ComprobanteId == id && x.Compra.Reserva.UsuarioId == usuarioId);
                if (c == null) return HttpNotFound();
                var vm = new ComprobanteFilaViewModel {
                    ComprobanteId = c.ComprobanteId, Numero = c.NumeroComprobante,
                    CodigoReserva = c.Compra.Reserva.CodigoReserva,
                    Fecha = c.FechaEmision, Importe = c.Compra.Importe,
                    Moneda = c.Compra.MonedaCodigo, Estado = c.EstadoComprobanteCodigo,
                    Tipo = c.TipoComprobanteCodigo
                };
                return View(vm);
            }
        }
    }
}
