using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AerolineaVIP_MVC.Models
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // Datos que acepta la aplicación: el usuario, costo y estado se determinan en el servidor.
    public class NuevaReservaViewModel
    {
        public int VueloId { get; set; }
        public string ResumenVuelo { get; set; }
        public decimal Tarifa { get; set; }
        [Required(ErrorMessage = "Seleccione el asiento.")]
        [Display(Name = "Asiento")]
        public byte? NumeroAsiento { get; set; }
        [Required(ErrorMessage = "Seleccione el tipo de documento.")]
        [Display(Name = "Tipo de documento")]
        public string TipoDocumentoCodigo { get; set; }
        [Required, StringLength(20, MinimumLength = 5)]
        [Display(Name = "Número de documento")]
        public string NumeroDocumento { get; set; }
        [Required, StringLength(100)]
        public string Nombres { get; set; }
        [Required, StringLength(100)]
        public string Apellidos { get; set; }
        [EmailAddress, StringLength(256)]
        public string Correo { get; set; }
        [RegularExpression(@"^[0-9+ ]*$", ErrorMessage = "Teléfono: solo dígitos, + o espacios.")]
        [StringLength(20)]
        public string Telefono { get; set; }
        public IEnumerable<SelectListItem> Asientos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TiposDocumento { get; set; } = new List<SelectListItem>();
    }

    public class ReservaResumenViewModel
    {
        public int ReservaId { get; set; }
        public string Codigo { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
        public int VueloId { get; set; }
        public DateTime FechaVuelo { get; set; }
        public string Pasajero { get; set; }
        public byte NumeroAsiento { get; set; }
        public string Asiento { get { return NumeroAsiento.ToString(); } }
        public string EstadoAsiento { get; set; }
        public decimal Importe { get; set; }
    }
}
