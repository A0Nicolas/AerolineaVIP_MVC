using System;
using System.Collections.Generic;

namespace AerolineaVIP_MVC.Models
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Modelos de presentación TEMPORALES para mostrar datos de lectura,
    // sin modificar las entidades que generó Entity Framework desde Model1.edmx.
    public class DiagnosticoBDViewModel
    {
        public int TotalAeronaves { get; set; }
        public int TotalVuelos { get; set; }
        public string Error { get; set; }
        public List<AeronaveResumen> Aeronaves { get; set; } = new List<AeronaveResumen>();
        public List<VueloResumen> Vuelos { get; set; } = new List<VueloResumen>();
    }

    public class AeronaveResumen
    {
        public int AeronaveId { get; set; }
        public string Matricula { get; set; }
        public string Estado { get; set; }
    }

    public class VueloResumen
    {
        public int VueloId { get; set; }
        public DateTime FechaOperacion { get; set; }
        public int? AeronaveId { get; set; }
        public string Estado { get; set; }
    }
}
