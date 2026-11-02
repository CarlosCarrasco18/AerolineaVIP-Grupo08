namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Asiento
    {
        public int ModeloAeronaveId { get; set; }
        public byte NumeroAsiento { get; set; }
        public string Codigo { get; set; }
        public byte Fila { get; set; }
        public string Lado { get; set; }
        public string Ubicacion { get; set; }
    
        public virtual ModeloAeronave ModeloAeronave { get; set; }
    }
}
