// CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
// ViewModels de consulta que no modifican las entidades del modelo EDMX.
using System;
using System.Collections.Generic;

namespace AerolineaVIP_MVC.Models
{
    public class CounterConsultaViewModel
    {
        public string CodigoBusqueda { get; set; }
        public string CodigoReserva { get; set; }
        public string EstadoReserva { get; set; }
        public DateTime? FechaReserva { get; set; }
        public string Mensaje { get; set; }
        public List<CounterPasajeroFila> Pasajeros { get; set; } = new List<CounterPasajeroFila>();
    }

    public class CounterPasajeroFila
    {
        public string Nombre { get; set; }
        public int VueloId { get; set; }
        public DateTime FechaVuelo { get; set; }
        public int Asiento { get; set; }
        public string Estado { get; set; }
    }

    public class GerenciaResumenViewModel
    {
        public int Vuelos { get; set; }
        public int Aeronaves { get; set; }
        public int Reservas { get; set; }
        public int Compras { get; set; }
    }
}
