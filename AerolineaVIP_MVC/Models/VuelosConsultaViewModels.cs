using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AerolineaVIP_MVC.Models
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Modelos de presentación; no modificar las entidades de Model1.edmx.
    public class VueloBusquedaViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de viaje")]
        public DateTime? Fecha { get; set; }

        [Display(Name = "Origen")]
        public int? OrigenId { get; set; }

        [Display(Name = "Destino")]
        public int? DestinoId { get; set; }

        public IEnumerable<SelectListItem> Aeropuertos { get; set; } = new List<SelectListItem>();
        public List<VueloConsultaFila> Resultados { get; set; } = new List<VueloConsultaFila>();
        public string Error { get; set; }
    }

    public class VueloConsultaFila
    {
        public int VueloId { get; set; }
        public DateTime FechaOperacion { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
        public TimeSpan HoraSalida { get; set; }
        public TimeSpan HoraLlegada { get; set; }
        public string Estado { get; set; }
        public string Matricula { get; set; }
        public decimal Tarifa { get; set; }
        public int Capacidad { get; set; }
        public int Ocupados { get; set; }
        public int Disponibles { get { return Math.Max(0, Capacidad - Ocupados); } }
    }

    public class VueloDetalleViewModel
    {
        public VueloConsultaFila Vuelo { get; set; }
        public List<AsientoConsultaFila> Asientos { get; set; } = new List<AsientoConsultaFila>();
        public string Error { get; set; }
    }

    public class AsientoConsultaFila
    {
        public byte NumeroAsiento { get; set; }
        public string Codigo { get; set; }
        public string Ubicacion { get; set; }
        public string EstadoVisual { get; set; }
    }
}
