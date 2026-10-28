using System;
using System.ComponentModel.DataAnnotations;

namespace Progra_03.Models
{
    [MetadataType(typeof(AeropuertoMetadata))]
    public partial class Aeropuerto
    {
    }

    public class AeropuertoMetadata
    {
        [Required(ErrorMessage = "Ingrese el código IATA.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "El código IATA debe tener 3 letras.")]
        [RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "El código IATA debe tener solo 3 letras.")]
        [Display(Name = "Código IATA")]
        public string CodigoIATA { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del aeropuerto.")]
        [StringLength(150, ErrorMessage = "El nombre admite máximo {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Ingrese la localidad.")]
        [StringLength(100, ErrorMessage = "La localidad admite máximo {1} caracteres.")]
        [Display(Name = "Localidad")]
        public string Localidad { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }
    }

    [MetadataType(typeof(RutaMetadata))]
    public partial class Ruta
    {
    }

    public class RutaMetadata
    {
        [Required(ErrorMessage = "Seleccione el aeropuerto de origen.")]
        [Display(Name = "Aeropuerto de origen")]
        public int AeropuertoOrigenId { get; set; }

        [Required(ErrorMessage = "Seleccione el aeropuerto de destino.")]
        [Display(Name = "Aeropuerto de destino")]
        public int AeropuertoDestinoId { get; set; }

        [Required(ErrorMessage = "Ingrese la tarifa referencial.")]
        [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "La tarifa debe estar entre {1} y {2}.")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Display(Name = "Tarifa referencial (USD)")]
        public decimal TarifaReferencial { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; }
    }

    [MetadataType(typeof(FrecuenciaMetadata))]
    public partial class Frecuencia
    {
    }

    public class FrecuenciaMetadata
    {
        [Required(ErrorMessage = "Seleccione la ruta.")]
        [Display(Name = "Ruta")]
        public int RutaId { get; set; }

        [Required(ErrorMessage = "Seleccione el día de operación.")]
        [Range(1, 7, ErrorMessage = "Seleccione un día válido.")]
        [Display(Name = "Día de la semana")]
        public byte DiaSemana { get; set; }

        [Required(ErrorMessage = "Ingrese la hora de salida.")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hora de salida")]
        public TimeSpan HoraSalida { get; set; }

        [Required(ErrorMessage = "Ingrese la hora de llegada.")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hora de llegada")]
        public TimeSpan HoraLlegada { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; }
    }

    [MetadataType(typeof(VueloMetadata))]
    public partial class Vuelo
    {
    }

    public class VueloMetadata
    {
        [Required(ErrorMessage = "Seleccione la frecuencia.")]
        [Display(Name = "Frecuencia")]
        public int FrecuenciaId { get; set; }

        [Required(ErrorMessage = "Ingrese la fecha de operación.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de operación")]
        public DateTime FechaOperacion { get; set; }

        [Display(Name = "Aeronave asignada")]
        public int? AeronaveId { get; set; }

        [Required(ErrorMessage = "Seleccione el estado del vuelo.")]
        [Display(Name = "Estado")]
        public string EstadoVueloCodigo { get; set; }
    }

    [MetadataType(typeof(ItinerarioMetadata))]
    public partial class Itinerario
    {
    }

    public class ItinerarioMetadata
    {
        [Required(ErrorMessage = "Ingrese la referencia.")]
        [StringLength(30, ErrorMessage = "La referencia admite máximo {1} caracteres.")]
        [Display(Name = "Referencia")]
        public string Referencia { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del itinerario.")]
        [StringLength(150, ErrorMessage = "El nombre admite máximo {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Seleccione el estado.")]
        [Display(Name = "Estado")]
        public string EstadoItinerarioCodigo { get; set; }
    }

    [MetadataType(typeof(TramoItinerarioMetadata))]
    public partial class TramoItinerario
    {
    }

    public class TramoItinerarioMetadata
    {
        [Required(ErrorMessage = "Seleccione el itinerario.")]
        [Display(Name = "Itinerario")]
        public int ItinerarioId { get; set; }

        [Required(ErrorMessage = "Seleccione el vuelo.")]
        [Display(Name = "Vuelo")]
        public int VueloId { get; set; }

        [Required(ErrorMessage = "Ingrese el orden del tramo.")]
        [Range(1, 10, ErrorMessage = "El orden debe estar entre {1} y {2}.")]
        [Display(Name = "Orden")]
        public byte Orden { get; set; }
    }

    [MetadataType(typeof(ModeloAeronaveMetadata))]
    public partial class ModeloAeronave
    {
    }

    public class ModeloAeronaveMetadata
    {
        [Required(ErrorMessage = "Ingrese el fabricante.")]
        [StringLength(60, ErrorMessage = "El fabricante admite máximo {1} caracteres.")]
        [Display(Name = "Fabricante")]
        public string Fabricante { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del modelo.")]
        [StringLength(60, ErrorMessage = "El modelo admite máximo {1} caracteres.")]
        [Display(Name = "Modelo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Ingrese la capacidad.")]
        [Range(1, 7, ErrorMessage = "La capacidad máxima es de 7 asientos.")]
        [Display(Name = "Capacidad de asientos")]
        public byte CapacidadAsientos { get; set; }
    }

    [MetadataType(typeof(AeronaveMetadata))]
    public partial class Aeronave
    {
    }

    public class AeronaveMetadata
    {
        [Required(ErrorMessage = "Ingrese la matrícula.")]
        [StringLength(15, ErrorMessage = "La matrícula admite máximo {1} caracteres.")]
        [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "La matrícula solo admite letras, números y guion.")]
        [Display(Name = "Matrícula")]
        public string Matricula { get; set; }

        [Required(ErrorMessage = "Seleccione el modelo.")]
        [Display(Name = "Modelo")]
        public int ModeloAeronaveId { get; set; }

        [Required(ErrorMessage = "Seleccione el estado.")]
        [Display(Name = "Estado operativo")]
        public string EstadoAeronaveCodigo { get; set; }
    }

    [MetadataType(typeof(PasajeroMetadata))]
    public partial class Pasajero
    {
    }

    public class PasajeroMetadata
    {
        [Required(ErrorMessage = "Seleccione el tipo de documento.")]
        [Display(Name = "Tipo de documento")]
        public string TipoDocumentoCodigo { get; set; }

        [Required(ErrorMessage = "Ingrese el número de documento.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "El documento debe tener entre {2} y {1} caracteres.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "El documento solo admite letras y números.")]
        [Display(Name = "Número de documento")]
        public string NumeroDocumento { get; set; }

        [Required(ErrorMessage = "Ingrese los nombres.")]
        [StringLength(100, ErrorMessage = "Los nombres admiten máximo {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "Los nombres solo admiten letras.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Ingrese los apellidos.")]
        [StringLength(100, ErrorMessage = "Los apellidos admiten máximo {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "Los apellidos solo admiten letras.")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(256)]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [RegularExpression(@"^[0-9+ ]{7,20}$", ErrorMessage = "El teléfono solo admite números, espacios y el signo +.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }
    }

    [MetadataType(typeof(ReservaMetadata))]
    public partial class Reserva
    {
    }

    public class ReservaMetadata
    {
        [Display(Name = "Código de reserva")]
        public string CodigoReserva { get; set; }

        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; }

        [Display(Name = "Fecha de creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Fecha de expiración")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? FechaExpiracion { get; set; }

        [Required(ErrorMessage = "Seleccione el estado.")]
        [Display(Name = "Estado")]
        public string EstadoReservaCodigo { get; set; }
    }

    [MetadataType(typeof(CompraMetadata))]
    public partial class Compra
    {
    }

    public class CompraMetadata
    {
        [Display(Name = "Reserva")]
        public int ReservaId { get; set; }

        [Display(Name = "Fecha de compra")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaCompra { get; set; }

        [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "El importe debe ser mayor a cero.")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        [Display(Name = "Moneda")]
        public string MonedaCodigo { get; set; }

        [Display(Name = "Estado")]
        public string EstadoCompraCodigo { get; set; }

        [Display(Name = "Clave de operación")]
        public Guid ClaveIdempotencia { get; set; }
    }

    [MetadataType(typeof(ComprobanteMetadata))]
    public partial class Comprobante
    {
    }

    public class ComprobanteMetadata
    {
        [Display(Name = "Número de comprobante")]
        public string NumeroComprobante { get; set; }

        [Display(Name = "Fecha de emisión")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaEmision { get; set; }

        [Display(Name = "Tipo")]
        public string TipoComprobanteCodigo { get; set; }

        [Display(Name = "Estado")]
        public string EstadoComprobanteCodigo { get; set; }
    }
}
