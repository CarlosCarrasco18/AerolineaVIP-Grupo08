using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Progra_03.Models
{
    [Serializable]
    public class SeleccionReserva
    {
        public SeleccionReserva()
        {
            VueloIds = new List<int>();
            Asientos = new List<byte>();
            PasajeroIds = new List<int>();
        }

        public List<int> VueloIds { get; set; }
        public int? ItinerarioId { get; set; }
        public int CantidadPasajeros { get; set; }
        public List<byte> Asientos { get; set; }
        public List<int> PasajeroIds { get; set; }
    }

    public class PagoViewModel
    {
        public int ReservaId { get; set; }

        [Required(ErrorMessage = "Seleccione el método de pago.")]
        [Display(Name = "Método de pago")]
        public string MetodoPagoCodigo { get; set; }

        [Required(ErrorMessage = "Seleccione la marca de la tarjeta.")]
        [Display(Name = "Marca")]
        public string MarcaTarjetaCodigo { get; set; }

        [Required(ErrorMessage = "Ingrese el número de tarjeta.")]
        [RegularExpression(@"^[0-9]{14,19}$", ErrorMessage = "El número de tarjeta solo admite dígitos.")]
        [Display(Name = "Número de tarjeta")]
        public string NumeroTarjeta { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del titular.")]
        [StringLength(100, ErrorMessage = "El nombre admite máximo {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El titular solo admite letras.")]
        [Display(Name = "Titular")]
        public string NombreTitular { get; set; }

        [Required(ErrorMessage = "Ingrese la fecha de vencimiento.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])/[0-9]{2}$", ErrorMessage = "Use el formato MM/AA.")]
        [Display(Name = "Vencimiento")]
        public string Vencimiento { get; set; }

        [Required(ErrorMessage = "Ingrese el código de seguridad.")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "El CVV tiene 3 o 4 dígitos.")]
        [DataType(DataType.Password)]
        [Display(Name = "CVV")]
        public string CodigoSeguridad { get; set; }
    }

    public class UsuarioViewModel
    {
        [Display(Name = "Usuario")]
        public string UserName { get; set; }

        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [Display(Name = "Roles")]
        public string Roles { get; set; }
    }
}
