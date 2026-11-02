namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Itinerario
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Itinerario()
        {
            this.TramoItinerarios = new HashSet<TramoItinerario>();
        }
    
        public int ItinerarioId { get; set; }
        public string Referencia { get; set; }
        public string Nombre { get; set; }
        public string EstadoItinerarioCodigo { get; set; }
    
        public virtual EstadoItinerario EstadoItinerario { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TramoItinerario> TramoItinerarios { get; set; }
    }
}
