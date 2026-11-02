namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EstadoCompra
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EstadoCompra()
        {
            this.Compras = new HashSet<Compra>();
        }
    
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Compra> Compras { get; set; }
    }
}
