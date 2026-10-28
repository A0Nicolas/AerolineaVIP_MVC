using System;
using System.Collections.Generic;

namespace AerolineaVIP_MVC.Models
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    public class CheckInFilaViewModel
    {
        public int AsignacionId { get; set; }
        public string CodigoReserva { get; set; }
        public string Pasajero { get; set; }
        public int NumeroAsiento { get; set; }
        public DateTime Salida { get; set; }
        public string Estado { get; set; }
        public bool PuedeCompletar { get; set; }
        public string Motivo { get; set; }
    }
    public class ComprobanteFilaViewModel
    {
        public int ComprobanteId { get; set; }
        public string Numero { get; set; }
        public string CodigoReserva { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public string Moneda { get; set; }
        public string Estado { get; set; }
        public string Tipo { get; set; }
    }
}
