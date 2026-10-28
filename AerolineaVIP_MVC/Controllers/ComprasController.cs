using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;
using Microsoft.AspNet.Identity;

namespace AerolineaVIP_MVC.Controllers
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Compra/pago académico de UNA reserva. No utiliza pasarela ni tarjetas reales.
    [Authorize]
    public class ComprasController : Controller
    {
        [HttpGet]
        public ActionResult Pagar(int reservaId)
        {
            using (var db = new AerolineaVIPEntities())
            {
                // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
                // Resolver el usuario antes de la consulta: EF6 no traduce GetUserId() a SQL.
                string usuarioId = User.Identity.GetUserId();
                var reserva = db.Reservas.SingleOrDefault(r => r.ReservaId == reservaId &&
                    r.UsuarioId == usuarioId);
                if (reserva == null) return HttpNotFound();
                if (reserva.EstadoReservaCodigo == "CONFIRMADA")
                    return RedirectToAction("Resultado", new { reservaId });
                if (reserva.EstadoReservaCodigo != "PENDIENTE")
                    return new HttpStatusCodeResult(HttpStatusCode.Conflict, "La reserva no admite pagos.");
                var vm = new PagoSimuladoViewModel { ReservaId = reservaId };
                Preparar(db, vm);
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pagar(PagoSimuladoViewModel vm)
        {
            using (var db = new AerolineaVIPEntities())
            {
                var usuarioId = User.Identity.GetUserId();
                var reserva = db.Reservas.SingleOrDefault(r => r.ReservaId == vm.ReservaId &&
                    r.UsuarioId == usuarioId);
                if (reserva == null) return HttpNotFound();
                if (reserva.EstadoReservaCodigo == "CONFIRMADA")
                    return RedirectToAction("Resultado", new { reservaId = vm.ReservaId });
                Preparar(db, vm);

                // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
                // La confirmación de pago ficticio se valida en servidor y no se
                // confía únicamente en el estado del checkbox del navegador.
                if (!vm.AceptaSimulacion)
                {
                    ModelState.AddModelError("AceptaSimulacion",
                        "Confirme que comprende que no existe un cobro real.");
                }

                if (!ModelState.IsValid) return View(vm);

                // Revalidar el método y la marca, sin confiar en el formulario.
                if (!db.MetodoPagoes.Any(m => m.Codigo == vm.MetodoPagoCodigo && m.Activo) ||
                    !db.MarcaTarjetas.Any(m => m.Codigo == vm.MarcaTarjetaCodigo && m.Activa))
                {
                    ModelState.AddModelError("", "Seleccione opciones activas de demostración.");
                    return View(vm);
                }

                // Aislamiento serializable más índices únicos SQL para impedir duplicados.
                using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        reserva = db.Reservas.SingleOrDefault(r => r.ReservaId == vm.ReservaId &&
                            r.UsuarioId == usuarioId);
                        if (reserva == null) return HttpNotFound();
                        if (reserva.EstadoReservaCodigo == "CONFIRMADA")
                            return RedirectToAction("Resultado", new { reservaId = vm.ReservaId });
                        if (reserva.EstadoReservaCodigo != "PENDIENTE" ||
                            (reserva.FechaExpiracion.HasValue && reserva.FechaExpiracion.Value <= DateTime.Now))
                        {
                            ModelState.AddModelError("", "La reserva ya no está pendiente o ha expirado.");
                            return View(vm);
                        }
                        var asignaciones = db.ReservaPasajeroVueloes
                            .Where(a => a.ReservaId == reserva.ReservaId).ToList();
                        if (asignaciones.Count == 0 ||
                            asignaciones.Any(a => a.EstadoAsignacionCodigo != "PENDIENTE"))
                        {
                            ModelState.AddModelError("", "La reserva no tiene asientos pendientes válidos.");
                            return View(vm);
                        }
                        // La cifra se calcula exclusivamente a partir de los tramos registrados.
                        decimal importe = asignaciones.Sum(a => a.ImporteTramo);
                        if (importe <= 0)
                        {
                            ModelState.AddModelError("", "La reserva no tiene un importe válido.");
                            return View(vm);
                        }
                        var compra = db.Compras.FirstOrDefault(c => c.ReservaId == reserva.ReservaId &&
                            (c.EstadoCompraCodigo == "PENDIENTE" || c.EstadoCompraCodigo == "PAGADA"));
                        if (compra != null && compra.EstadoCompraCodigo == "PAGADA")
                            return RedirectToAction("Resultado", new { reservaId = vm.ReservaId });
                        if (compra != null && db.Pagoes.Any(p => p.CompraId == compra.CompraId && p.EstadoPagoCodigo == "APROBADO"))
                            return RedirectToAction("Resultado", new { reservaId = vm.ReservaId });
                        if (compra == null)
                        {
                            compra = new Compra
                            {
                                ReservaId = reserva.ReservaId,
                                FechaCompra = DateTime.Now,
                                Importe = importe,
                                MonedaCodigo = "USD",
                                EstadoCompraCodigo = "PENDIENTE",
                                ClaveIdempotencia = Guid.NewGuid()
                            };
                            db.Compras.Add(compra);
                            db.SaveChanges();
                        }
                        // Pago ficticio: NUNCA introducir ni guardar datos financieros reales.
                        var pago = new Pago
                        {
                            CompraId = compra.CompraId,
                            FechaPago = DateTime.Now,
                            Importe = compra.Importe,
                            EstadoPagoCodigo = "APROBADO",
                            MetodoPagoCodigo = vm.MetodoPagoCodigo,
                            MarcaTarjetaCodigo = vm.MarcaTarjetaCodigo,
                            UltimosCuatroDigitos = "0000",
                            NombreTitular = "DEMO ACADEMICA",
                            CodigoOperacion = "SIM-" + Guid.NewGuid().ToString("N").ToUpperInvariant(),
                            EsSimulado = true,
                            MensajeResultado = "Pago ficticio aprobado, sin cobro real"
                        };
                        db.Pagoes.Add(pago);
                        db.SaveChanges();
                        compra.EstadoCompraCodigo = "PAGADA";
                        reserva.EstadoReservaCodigo = "CONFIRMADA";
                        foreach (var a in asignaciones) a.EstadoAsignacionCodigo = "CONFIRMADA";
                        db.Comprobantes.Add(new Comprobante
                        {
                            CompraId = compra.CompraId,
                            TipoComprobanteCodigo = "CONFIRMACION",
                            NumeroComprobante = "ACAD-" + compra.CompraId.ToString("D8"),
                            FechaEmision = DateTime.Now,
                            EstadoComprobanteCodigo = "EMITIDO"
                        });
                        db.SaveChanges();
                        tx.Commit();
                        return RedirectToAction("Resultado", new { reservaId = reserva.ReservaId });
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        Trace.TraceError("Compras/Pagar: " + ex);
                        ModelState.AddModelError("", "No se pudo registrar la operación simulada. Verifique que la reserva siga disponible y vuelva a consultar su estado.");
                        return View(vm);
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult Resultado(int reservaId)
        {
            using (var db = new AerolineaVIPEntities())
            {
                var usuarioId = User.Identity.GetUserId();
                var datos = (from r in db.Reservas
                             join c in db.Compras on r.ReservaId equals c.ReservaId
                             where r.ReservaId == reservaId && r.UsuarioId == usuarioId && c.EstadoCompraCodigo == "PAGADA"
                             select new ResultadoCompraViewModel
                             {
                                 ReservaId = r.ReservaId,
                                 CodigoReserva = r.CodigoReserva,
                                 CompraId = c.CompraId,
                                 EstadoCompra = c.EstadoCompraCodigo,
                                 EstadoReserva = r.EstadoReservaCodigo,
                                 Importe = c.Importe,
                                 Moneda = c.MonedaCodigo,
                                 Fecha = c.FechaCompra
                             }).FirstOrDefault();
                if (datos == null) return HttpNotFound();
                var pago = db.Pagoes.Where(p => p.CompraId == datos.CompraId && p.EstadoPagoCodigo == "APROBADO")
                    .OrderByDescending(p => p.PagoId).FirstOrDefault();
                var comp = db.Comprobantes.Where(c => c.CompraId == datos.CompraId &&
                    c.TipoComprobanteCodigo == "CONFIRMACION")
                    .OrderByDescending(c => c.ComprobanteId).FirstOrDefault();
                if (pago != null)
                {
                    datos.CodigoOperacion = pago.CodigoOperacion;
                    datos.Metodo = pago.MetodoPagoCodigo;
                    datos.Marca = pago.MarcaTarjetaCodigo;
                    datos.UltimosCuatro = pago.UltimosCuatroDigitos;
                }
                if (comp != null)
                {
                    datos.NumeroComprobante = comp.NumeroComprobante;
                    datos.TipoComprobante = comp.TipoComprobanteCodigo;
                    datos.EstadoComprobante = comp.EstadoComprobanteCodigo;
                }
                return View(datos);
            }
        }

        private static void Preparar(AerolineaVIPEntities db, PagoSimuladoViewModel vm)
        {
            var reserva = db.Reservas.FirstOrDefault(r => r.ReservaId == vm.ReservaId);
            vm.CodigoReserva = reserva == null ? "" : reserva.CodigoReserva;
            vm.EstadoReserva = reserva == null ? "" : reserva.EstadoReservaCodigo;
            vm.Importe = db.ReservaPasajeroVueloes.Where(a => a.ReservaId == vm.ReservaId)
                .Select(a => (decimal?)a.ImporteTramo).Sum() ?? 0m;
            vm.Metodos = db.MetodoPagoes.Where(m => m.Activo).OrderBy(m => m.Nombre).ToList()
                .Select(m => new SelectListItem { Value = m.Codigo, Text = m.Nombre }).ToList();
            vm.Marcas = db.MarcaTarjetas.Where(m => m.Activa).OrderBy(m => m.Nombre).ToList()
                .Select(m => new SelectListItem { Value = m.Codigo, Text = m.Nombre }).ToList();
        }
    }
}
