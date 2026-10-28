using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;
using Microsoft.AspNet.Identity;

namespace AerolineaVIP_MVC.Controllers
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Nico: reservas de UN pasajero para UN vuelo por operación.
    [Authorize]
    public class ReservasController : Controller
    {
        [HttpGet]
        public ActionResult Nueva(int vueloId)
        {
            var vm = new NuevaReservaViewModel { VueloId = vueloId };
            using (var db = new AerolineaVIPEntities())
            {
                if (!PrepararFormulario(db, vm)) return HttpNotFound();
            }
            return View(vm);
        }

        // CÓDIGO PROPIO: transacción serializable para impedir confirmar una plaza
        // que otro usuario ha reservado entre la consulta y el POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Nueva(NuevaReservaViewModel vm)
        {
            var usuarioId = User.Identity.GetUserId();
            if (string.IsNullOrWhiteSpace(usuarioId)) return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            vm.NumeroDocumento = (vm.NumeroDocumento ?? "").Trim();
            vm.Nombres = (vm.Nombres ?? "").Trim();
            vm.Apellidos = (vm.Apellidos ?? "").Trim();
            vm.TipoDocumentoCodigo = (vm.TipoDocumentoCodigo ?? "").Trim();
            vm.Correo = string.IsNullOrWhiteSpace(vm.Correo) ? null : vm.Correo.Trim();
            vm.Telefono = string.IsNullOrWhiteSpace(vm.Telefono) ? null : vm.Telefono.Trim();

            using (var db = new AerolineaVIPEntities())
            {
                if (!PrepararFormulario(db, vm)) return HttpNotFound();
                if (!ModelState.IsValid) return View(vm);

                // Las consultas para verificar vuelo, asiento y pasajero se repiten
                // DENTRO de la transacción. No confiar en valores enviados por HTML.
                using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        var vuelo = db.Vueloes.Include(v => v.Aeronave.ModeloAeronave)
                            .Include(v => v.Frecuencia.Ruta)
                            .SingleOrDefault(v => v.VueloId == vm.VueloId);
                        if (vuelo == null || vuelo.Aeronave == null ||
                            vuelo.EstadoVueloCodigo != "PROGRAMADO" || vuelo.FechaOperacion < DateTime.Today ||
                            vuelo.Frecuencia == null || !vuelo.Frecuencia.Activa ||
                            vuelo.Frecuencia.Ruta == null || !vuelo.Frecuencia.Ruta.Activa)
                        {
                            ModelState.AddModelError("", "El vuelo no está disponible para reservas.");
                            return View(vm);
                        }
                        var numero = vm.NumeroAsiento.Value;
                        bool asientoExiste = db.Asientoes.Any(s =>
                            s.ModeloAeronaveId == vuelo.Aeronave.ModeloAeronaveId && s.NumeroAsiento == numero);
                        if (!asientoExiste || numero < 1 || numero > 7)
                        {
                            ModelState.AddModelError("NumeroAsiento", "El asiento no pertenece a esta aeronave.");
                            return View(vm);
                        }
                        bool ocupado = db.ReservaPasajeroVueloes.Any(a => a.VueloId == vm.VueloId &&
                            a.NumeroAsiento == numero &&
                            (a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA"));
                        if (ocupado)
                        {
                            ModelState.AddModelError("NumeroAsiento", "El asiento ya está reservado; elija otro.");
                            return View(vm);
                        }
                        if (!db.TipoDocumentoIdentidads.Any(t => t.Codigo == vm.TipoDocumentoCodigo))
                        {
                            ModelState.AddModelError("TipoDocumentoCodigo", "Tipo de documento inválido.");
                            return View(vm);
                        }
                        var pasajero = db.Pasajeroes.SingleOrDefault(p =>
                            p.TipoDocumentoCodigo == vm.TipoDocumentoCodigo && p.NumeroDocumento == vm.NumeroDocumento);
                        if (pasajero != null)
                        {
                            // Evita que se sustituyan los datos de un pasajero existente
                            // introduciendo solamente su documento.
                            if (!string.Equals(pasajero.Nombres.Trim(), vm.Nombres, StringComparison.OrdinalIgnoreCase) ||
                                !string.Equals(pasajero.Apellidos.Trim(), vm.Apellidos, StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError("NumeroDocumento", "El documento ya está registrado con otros nombres. Verifique los datos.");
                                return View(vm);
                            }
                            if (db.ReservaPasajeroVueloes.Any(a => a.VueloId == vm.VueloId && a.PasajeroId == pasajero.PasajeroId &&
                                (a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA")))
                            {
                                ModelState.AddModelError("", "Este pasajero ya tiene una reserva activa en el vuelo.");
                                return View(vm);
                            }
                        }
                        else
                        {
                            pasajero = new Pasajero
                            {
                                TipoDocumentoCodigo = vm.TipoDocumentoCodigo, NumeroDocumento = vm.NumeroDocumento,
                                Nombres = vm.Nombres, Apellidos = vm.Apellidos, Correo = vm.Correo,
                                Telefono = vm.Telefono, FechaRegistro = DateTime.Now
                            };
                            db.Pasajeroes.Add(pasajero);
                        }
                        var reserva = new Reserva
                        {
                            UsuarioId = usuarioId, FechaCreacion = DateTime.Now,
                            EstadoReservaCodigo = "PENDIENTE"
                        };
                        db.Reservas.Add(reserva);
                        // Solo el servidor calcula la tarifa referencial. Nunca usar
                        // un valor de precio o usuario recibido en el formulario.
                        var asignacion = new ReservaPasajeroVuelo
                        {
                            Reserva = reserva, Pasajero = pasajero, VueloId = vuelo.VueloId,
                            NumeroAsiento = numero, ImporteTramo = vuelo.Frecuencia.Ruta.TarifaReferencial,
                            EstadoAsignacionCodigo = "PENDIENTE", FechaAsignacion = DateTime.Now
                        };
                        db.ReservaPasajeroVueloes.Add(asignacion);
                        db.SaveChanges();
                        tx.Commit();
                        return RedirectToAction("Detalle", new { id = reserva.ReservaId });
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        Trace.TraceError("Reservas/Nueva: " + ex);
                        ModelState.AddModelError("", "No se pudo registrar la reserva. Puede que el asiento ya haya sido tomado o los datos sean inválidos. Actualice e inténtelo nuevamente.");
                        PrepararFormulario(db, vm);
                        return View(vm);
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult MisReservas()
        {
            var usuarioId = User.Identity.GetUserId();
            using (var db = new AerolineaVIPEntities())
            {
                var filas = (from r in db.Reservas
                             join a in db.ReservaPasajeroVueloes on r.ReservaId equals a.ReservaId
                             join p in db.Pasajeroes on a.PasajeroId equals p.PasajeroId
                             join v in db.Vueloes on a.VueloId equals v.VueloId
                             where r.UsuarioId == usuarioId
                             orderby r.FechaCreacion descending
                             select new ReservaResumenViewModel
                             {
                                 ReservaId = r.ReservaId, Codigo = r.CodigoReserva,
                                 Estado = r.EstadoReservaCodigo, Fecha = r.FechaCreacion,
                                 VueloId = v.VueloId, FechaVuelo = v.FechaOperacion,
                                 Pasajero = p.Nombres + " " + p.Apellidos,
                                 EstadoAsiento = a.EstadoAsignacionCodigo, Importe = a.ImporteTramo,
                                 NumeroAsiento = a.NumeroAsiento
                             }).Take(100).ToList();
                return View(filas);
            }
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            var usuarioId = User.Identity.GetUserId();
            using (var db = new AerolineaVIPEntities())
            {
                var fila = (from r in db.Reservas
                            join a in db.ReservaPasajeroVueloes on r.ReservaId equals a.ReservaId
                            join p in db.Pasajeroes on a.PasajeroId equals p.PasajeroId
                            join v in db.Vueloes on a.VueloId equals v.VueloId
                            where r.ReservaId == id && r.UsuarioId == usuarioId
                            select new ReservaResumenViewModel
                            {
                                ReservaId = r.ReservaId, Codigo = r.CodigoReserva,
                                Estado = r.EstadoReservaCodigo, Fecha = r.FechaCreacion,
                                VueloId = v.VueloId, FechaVuelo = v.FechaOperacion,
                                Pasajero = p.Nombres + " " + p.Apellidos,
                                EstadoAsiento = a.EstadoAsignacionCodigo, Importe = a.ImporteTramo,
                                NumeroAsiento = a.NumeroAsiento
                            }).FirstOrDefault();
                if (fila == null) return HttpNotFound();
                return View(fila);
            }
        }

        private static bool PrepararFormulario(AerolineaVIPEntities db, NuevaReservaViewModel vm)
        {
            var vuelo = db.Vueloes.Include(v => v.Aeronave.ModeloAeronave)
                .Include(v => v.Frecuencia.Ruta).SingleOrDefault(v => v.VueloId == vm.VueloId);
            if (vuelo == null) return false;
            vm.ResumenVuelo = "VIP" + vuelo.VueloId.ToString("D5") + " · " + vuelo.FechaOperacion.ToString("dd/MM/yyyy");
            vm.Tarifa = vuelo.Frecuencia.Ruta.TarifaReferencial;
            vm.TiposDocumento = db.TipoDocumentoIdentidads.OrderBy(t => t.Codigo)
                .ToList().Select(t => new SelectListItem { Value = t.Codigo, Text = t.Descripcion }).ToList();
            if (vuelo.Aeronave != null && vuelo.EstadoVueloCodigo == "PROGRAMADO" && vuelo.FechaOperacion >= DateTime.Today)
            {
                var ocupados = db.ReservaPasajeroVueloes.Where(a => a.VueloId == vm.VueloId &&
                    (a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA"))
                    .Select(a => a.NumeroAsiento).ToList();
                vm.Asientos = db.Asientoes.Where(s => s.ModeloAeronaveId == vuelo.Aeronave.ModeloAeronaveId)
                    .OrderBy(s => s.NumeroAsiento).ToList()
                    .Where(s => !ocupados.Contains(s.NumeroAsiento))
                    .Select(s => new SelectListItem { Value = s.NumeroAsiento.ToString(), Text = s.Codigo + " — " + s.Ubicacion }).ToList();
            }
            return true;
        }
    }
}
