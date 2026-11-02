namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DatosEmisor
    {
        public byte DatosEmisorId { get; set; }
        public string NombreComercial { get; set; }
        public string Identificacion { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string LeyendaComprobante { get; set; }
    }
}
