namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TipoDocumentoIdentidad
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TipoDocumentoIdentidad()
        {
            this.Pasajeroes = new HashSet<Pasajero>();
        }
    
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pasajero> Pasajeroes { get; set; }
    }
}
