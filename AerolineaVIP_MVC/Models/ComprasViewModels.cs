using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AerolineaVIP_MVC.Models
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Pago 100 % académico: no se solicitan ni almacenan datos reales de tarjeta.
    public class PagoSimuladoViewModel
    {
        public int ReservaId { get; set; }
        public string CodigoReserva { get; set; }
        public decimal Importe { get; set; }
        public string EstadoReserva { get; set; }
        [Required(ErrorMessage = "Seleccione un método de pago de demostración.")]
        [Display(Name = "Método de pago simulado")]
        public string MetodoPagoCodigo { get; set; }
        [Required(ErrorMessage = "Seleccione una marca de demostración.")]
        [Display(Name = "Marca de tarjeta (simulación)")]
        public string MarcaTarjetaCodigo { get; set; }
        // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
        // Esta confirmación booleana se valida explícitamente en el POST del controlador.
        // No usar [Range] para bool: jQuery Unobtrusive Validation interpreta
        // la regla como rango numérico y puede rechazar una casilla marcada.
        public bool AceptaSimulacion { get; set; }
        public IEnumerable<SelectListItem> Metodos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Marcas { get; set; } = new List<SelectListItem>();
    }

    public class ResultadoCompraViewModel
    {
        public int ReservaId { get; set; }
        public string CodigoReserva { get; set; }
        public int CompraId { get; set; }
        public string EstadoCompra { get; set; }
        public string EstadoReserva { get; set; }
        public decimal Importe { get; set; }
        public string Moneda { get; set; }
        public DateTime Fecha { get; set; }
        public string CodigoOperacion { get; set; }
        public string Metodo { get; set; }
        public string Marca { get; set; }
        public string UltimosCuatro { get; set; }
        public string NumeroComprobante { get; set; }
        public string TipoComprobante { get; set; }
        public string EstadoComprobante { get; set; }
    }
}
