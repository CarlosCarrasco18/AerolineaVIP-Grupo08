namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EstadoAsignacion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EstadoAsignacion()
        {
            this.ReservaPasajeroVueloes = new HashSet<ReservaPasajeroVuelo>();
        }
    
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public bool OcupaInventario { get; set; }
        public string EstadoVisual { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReservaPasajeroVuelo> ReservaPasajeroVueloes { get; set; }
    }
}
