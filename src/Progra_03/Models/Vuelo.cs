namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Vuelo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Vuelo()
        {
            this.ReservaPasajeroVueloes = new HashSet<ReservaPasajeroVuelo>();
            this.TramoItinerarios = new HashSet<TramoItinerario>();
        }
    
        public int VueloId { get; set; }
        public int FrecuenciaId { get; set; }
        public System.DateTime FechaOperacion { get; set; }
        public Nullable<int> AeronaveId { get; set; }
        public string EstadoVueloCodigo { get; set; }
    
        public virtual Aeronave Aeronave { get; set; }
        public virtual EstadoVuelo EstadoVuelo { get; set; }
        public virtual Frecuencia Frecuencia { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReservaPasajeroVuelo> ReservaPasajeroVueloes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TramoItinerario> TramoItinerarios { get; set; }
    }
}
