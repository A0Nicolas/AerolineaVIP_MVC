using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;

namespace AerolineaVIP_MVC.Controllers
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Nico: búsqueda pública de vuelos y lectura de inventario.
    // No altera las pantallas que desarrollará Mela.
    public class VuelosController : Controller
    {
        [HttpGet]
        public ActionResult Index(DateTime? fecha, int? origenId, int? destinoId, string origenCodigo = null, string destinoCodigo = null)
        {
            var vm = new VueloBusquedaViewModel
            {
                Fecha = fecha,
                OrigenId = origenId,
                DestinoId = destinoId
            };

            try
            {
                using (var db = new AerolineaVIPEntities())
                {
                    // CÓDIGO PROPIO: vinculación segura del buscador visual de Mela con las claves SQL reales.
                    // No asumir los ID de aeropuertos sembrados en otras máquinas.
                    if (!string.IsNullOrWhiteSpace(origenCodigo))
                    {
                        string codigo = origenCodigo.Trim().ToUpperInvariant();
                        vm.OrigenId = db.Aeropuertoes.Where(a => a.CodigoIATA == codigo && a.Activo)
                            .Select(a => (int?)a.AeropuertoId).FirstOrDefault();
                    }
                    if (!string.IsNullOrWhiteSpace(destinoCodigo))
                    {
                        string codigo = destinoCodigo.Trim().ToUpperInvariant();
                        vm.DestinoId = db.Aeropuertoes.Where(a => a.CodigoIATA == codigo && a.Activo)
                            .Select(a => (int?)a.AeropuertoId).FirstOrDefault();
                    }
                    origenId = vm.OrigenId;
                    destinoId = vm.DestinoId;

                    vm.Aeropuertos = db.Aeropuertoes
                        .Where(a => a.Activo)
                        .OrderBy(a => a.CodigoIATA)
                        .Select(a => new { a.AeropuertoId, a.CodigoIATA, a.Localidad })
                        .ToList()
                        .Select(a => new SelectListItem
                        {
                            Value = a.AeropuertoId.ToString(),
                            Text = a.CodigoIATA + " - " + a.Localidad
                        }).ToList();

                    // CÓDIGO PROPIO: filtrar en SQL antes de materializar.
                    var consulta = from v in db.Vueloes
                                   join f in db.Frecuencias on v.FrecuenciaId equals f.FrecuenciaId
                                   join r in db.Rutas on f.RutaId equals r.RutaId
                                   join origen in db.Aeropuertoes on r.AeropuertoOrigenId equals origen.AeropuertoId
                                   join destino in db.Aeropuertoes on r.AeropuertoDestinoId equals destino.AeropuertoId
                                   where v.EstadoVueloCodigo == "PROGRAMADO" &&
                                         v.FechaOperacion >= DateTime.Today &&
                                         f.Activa && r.Activa && origen.Activo && destino.Activo
                                   select new
                                   {
                                       v.VueloId, v.FechaOperacion, v.AeronaveId, v.EstadoVueloCodigo,
                                       f.HoraSalida, f.HoraLlegada,
                                       r.AeropuertoOrigenId, r.AeropuertoDestinoId, r.TarifaReferencial,
                                       Origen = origen.CodigoIATA,
                                       Destino = destino.CodigoIATA
                                   };

                    if (fecha.HasValue)
                    {
                        var dia = fecha.Value.Date;
                        consulta = consulta.Where(x => DbFunctions.TruncateTime(x.FechaOperacion) == dia);
                    }
                    if (origenId.HasValue) consulta = consulta.Where(x => x.AeropuertoOrigenId == origenId.Value);
                    if (destinoId.HasValue) consulta = consulta.Where(x => x.AeropuertoDestinoId == destinoId.Value);

                    // Limitar la pantalla de consulta, sin afectar el inventario en BD.
                    var vuelos = consulta.OrderBy(x => x.FechaOperacion)
                        .ThenBy(x => x.HoraSalida).ThenBy(x => x.VueloId)
                        .Take(100).ToList();

                    var aeronaves = db.Aeronaves
                        .Select(a => new { a.AeronaveId, a.Matricula, Capacidad = a.ModeloAeronave.CapacidadAsientos })
                        .ToList().ToDictionary(a => a.AeronaveId);
                    var ids = vuelos.Select(v => v.VueloId).ToList();
                    var ocupados = db.ReservaPasajeroVueloes
                        .Where(a => ids.Contains(a.VueloId) &&
                            (a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA"))
                        .GroupBy(a => a.VueloId)
                        .Select(g => new { VueloId = g.Key, Cantidad = g.Count() })
                        .ToList().ToDictionary(g => g.VueloId, g => g.Cantidad);

                    foreach (var v in vuelos)
                    {
                        var avion = v.AeronaveId.HasValue && aeronaves.ContainsKey(v.AeronaveId.Value)
                            ? aeronaves[v.AeronaveId.Value] : null;
                        var usados = ocupados.ContainsKey(v.VueloId) ? ocupados[v.VueloId] : 0;
                        vm.Resultados.Add(new VueloConsultaFila
                        {
                            VueloId = v.VueloId,
                            FechaOperacion = v.FechaOperacion,
                            Origen = v.Origen.Trim(),
                            Destino = v.Destino.Trim(),
                            HoraSalida = v.HoraSalida,
                            HoraLlegada = v.HoraLlegada,
                            Estado = v.EstadoVueloCodigo,
                            Matricula = avion == null ? "Sin asignar" : avion.Matricula,
                            Capacidad = avion == null ? 0 : avion.Capacidad,
                            Ocupados = usados,
                            Tarifa = v.TarifaReferencial
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Vuelos/Index: " + ex);
                vm.Error = "No fue posible consultar los vuelos. Revisa la conexión a SQL Server.";
            }
            return View(vm);
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            var vm = new VueloDetalleViewModel();
            try
            {
                using (var db = new AerolineaVIPEntities())
                {
                    var vuelo = (from v in db.Vueloes
                                 join f in db.Frecuencias on v.FrecuenciaId equals f.FrecuenciaId
                                 join r in db.Rutas on f.RutaId equals r.RutaId
                                 join o in db.Aeropuertoes on r.AeropuertoOrigenId equals o.AeropuertoId
                                 join d in db.Aeropuertoes on r.AeropuertoDestinoId equals d.AeropuertoId
                                 where v.VueloId == id
                                 select new
                                 {
                                     v.VueloId, v.FechaOperacion, v.AeronaveId, v.EstadoVueloCodigo,
                                     f.HoraSalida, f.HoraLlegada, r.TarifaReferencial,
                                     Origen = o.CodigoIATA, Destino = d.CodigoIATA
                                 }).FirstOrDefault();
                    if (vuelo == null) return HttpNotFound();

                    var aeronave = vuelo.AeronaveId.HasValue
                        ? db.Aeronaves.Include(a => a.ModeloAeronave)
                            .FirstOrDefault(a => a.AeronaveId == vuelo.AeronaveId.Value)
                        : null;

                    var asignaciones = db.ReservaPasajeroVueloes
                        .Where(a => a.VueloId == id && (a.EstadoAsignacionCodigo == "PENDIENTE" ||
                                                         a.EstadoAsignacionCodigo == "CONFIRMADA"))
                        .Select(a => new { a.NumeroAsiento, a.EstadoAsignacionCodigo })
                        .ToList().ToDictionary(a => a.NumeroAsiento, a => a.EstadoAsignacionCodigo);

                    vm.Vuelo = new VueloConsultaFila
                    {
                        VueloId = vuelo.VueloId, FechaOperacion = vuelo.FechaOperacion,
                        Origen = vuelo.Origen.Trim(), Destino = vuelo.Destino.Trim(),
                        HoraSalida = vuelo.HoraSalida, HoraLlegada = vuelo.HoraLlegada,
                        Tarifa = vuelo.TarifaReferencial, Estado = vuelo.EstadoVueloCodigo,
                        Matricula = aeronave == null ? "Sin asignar" : aeronave.Matricula,
                        Capacidad = aeronave == null ? 0 : aeronave.ModeloAeronave.CapacidadAsientos,
                        Ocupados = asignaciones.Count
                    };

                    if (aeronave != null)
                    {
                        var asientos = db.Asientoes
                            .Where(a => a.ModeloAeronaveId == aeronave.ModeloAeronaveId)
                            .OrderBy(a => a.NumeroAsiento)
                            .ToList();
                        foreach (var asiento in asientos)
                        {
                            var estado = asignaciones.ContainsKey(asiento.NumeroAsiento)
                                ? (asignaciones[asiento.NumeroAsiento] == "CONFIRMADA" ? "OCUPADO" : "RESERVADO")
                                : "DISPONIBLE";
                            vm.Asientos.Add(new AsientoConsultaFila
                            {
                                NumeroAsiento = asiento.NumeroAsiento,
                                Codigo = asiento.Codigo.Trim(),
                                Ubicacion = asiento.Ubicacion,
                                EstadoVisual = estado
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Vuelos/Detalle: " + ex);
                vm.Error = "No fue posible cargar la disponibilidad de asientos.";
            }
            return View(vm);
        }
    }
}
