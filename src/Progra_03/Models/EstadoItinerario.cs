namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EstadoItinerario
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EstadoItinerario()
        {
            this.Itinerarios = new HashSet<Itinerario>();
        }
    
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Itinerario> Itinerarios { get; set; }
    }
}
