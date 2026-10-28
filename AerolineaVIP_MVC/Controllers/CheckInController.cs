using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;
using Microsoft.AspNet.Identity;

namespace AerolineaVIP_MVC.Controllers
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Check-in académico: reserva confirmada, titular y ventana de 24 a 1 h antes del vuelo.
    [Authorize]
    public class CheckInController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            string usuarioId = User.Identity.GetUserId();
            using (var db = new AerolineaVIPEntities())
            {
                var registros = db.ReservaPasajeroVueloes
                    .Include(a => a.Reserva)
                    .Include(a => a.Pasajero)
                    .Include(a => a.Vuelo.Frecuencia)
                    .Include(a => a.CheckIns)
                    .Where(a => a.Reserva.UsuarioId == usuarioId &&
                        a.Reserva.EstadoReservaCodigo == "CONFIRMADA")
                    .OrderByDescending(a => a.AsignacionId).Take(60).ToList();
                var lista = new List<CheckInFilaViewModel>();
                foreach (var a in registros)
                {
                    DateTime salida = a.Vuelo.FechaOperacion.Date.Add(a.Vuelo.Frecuencia.HoraSalida);
                    bool realizado = a.CheckIns.Any(c => c.EstadoCheckInCodigo == "COMPLETADO");
                    bool ventana = DateTime.Now >= salida.AddHours(-24) && DateTime.Now <= salida.AddHours(-1);
                    lista.Add(new CheckInFilaViewModel {
                        AsignacionId = a.AsignacionId, CodigoReserva = a.Reserva.CodigoReserva,
                        Pasajero = a.Pasajero.Nombres + " " + a.Pasajero.Apellidos,
                        NumeroAsiento = a.NumeroAsiento, Salida = salida,
                        Estado = realizado ? "COMPLETADO" : "PENDIENTE",
                        PuedeCompletar = !realizado && ventana && a.EstadoAsignacionCodigo == "CONFIRMADA",
                        Motivo = realizado ? "Check-in ya realizado" :
                            !ventana ? "Disponible desde 24 h hasta 1 h antes de la salida" : "Habilitado"
                    });
                }
                return View(lista);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Completar(int asignacionId)
        {
            string usuarioId = User.Identity.GetUserId();
            using (var db = new AerolineaVIPEntities())
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    var a = db.ReservaPasajeroVueloes.Include(x => x.Reserva)
                        .Include(x => x.Vuelo.Frecuencia)
                        .SingleOrDefault(x => x.AsignacionId == asignacionId && x.Reserva.UsuarioId == usuarioId);
                    if (a == null) return HttpNotFound();
                    DateTime salida = a.Vuelo.FechaOperacion.Date.Add(a.Vuelo.Frecuencia.HoraSalida);
                    if (a.Reserva.EstadoReservaCodigo != "CONFIRMADA" ||
                        a.EstadoAsignacionCodigo != "CONFIRMADA" ||
                        DateTime.Now < salida.AddHours(-24) || DateTime.Now > salida.AddHours(-1))
                    {
                        TempData["Error"] = "El check-in no cumple las condiciones de estado o la ventana horaria.";
                        return RedirectToAction("Index");
                    }
                    if (db.CheckIns.Any(x => x.AsignacionId == asignacionId && x.EstadoCheckInCodigo == "COMPLETADO"))
                    {
                        TempData["Error"] = "El check-in ya está realizado.";
                        return RedirectToAction("Index");
                    }
                    db.CheckIns.Add(new CheckIn {
                        AsignacionId = asignacionId, FechaRegistro = DateTime.Now,
                        EstadoCheckInCodigo = "COMPLETADO"
                    });
                    db.SaveChanges();
                    tx.Commit();
                    TempData["Exito"] = "Check-in completado correctamente.";
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    Trace.TraceError("CheckIn/Completar: " + ex);
                    TempData["Error"] = "No se pudo realizar el check-in. Compruebe la reserva e inténtelo nuevamente.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
