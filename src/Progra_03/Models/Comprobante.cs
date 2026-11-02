namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Comprobante
    {
        public int ComprobanteId { get; set; }
        public int CompraId { get; set; }
        public string TipoComprobanteCodigo { get; set; }
        public string NumeroComprobante { get; set; }
        public System.DateTime FechaEmision { get; set; }
        public string EstadoComprobanteCodigo { get; set; }
    
        public virtual Compra Compra { get; set; }
        public virtual EstadoComprobante EstadoComprobante { get; set; }
        public virtual TipoComprobante TipoComprobante { get; set; }
    }
}
