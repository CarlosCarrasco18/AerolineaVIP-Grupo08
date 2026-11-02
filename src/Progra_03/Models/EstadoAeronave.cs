namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EstadoAeronave
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EstadoAeronave()
        {
            this.Aeronaves = new HashSet<Aeronave>();
        }
    
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Aeronave> Aeronaves { get; set; }
    }
}
