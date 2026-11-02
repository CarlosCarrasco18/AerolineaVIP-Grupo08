namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EstadoVuelo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EstadoVuelo()
        {
            this.Vueloes = new HashSet<Vuelo>();
        }
    
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vuelo> Vueloes { get; set; }
    }
}
