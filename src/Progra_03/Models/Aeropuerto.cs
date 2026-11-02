namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Aeropuerto
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Aeropuerto()
        {
            this.Rutas = new HashSet<Ruta>();
            this.Rutas1 = new HashSet<Ruta>();
        }
    
        public int AeropuertoId { get; set; }
        public string CodigoIATA { get; set; }
        public string Nombre { get; set; }
        public string Localidad { get; set; }
        public bool Activo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ruta> Rutas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ruta> Rutas1 { get; set; }
    }
}
