namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Ruta
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Ruta()
        {
            this.Frecuencias = new HashSet<Frecuencia>();
        }
    
        public int RutaId { get; set; }
        public int AeropuertoOrigenId { get; set; }
        public int AeropuertoDestinoId { get; set; }
        public decimal TarifaReferencial { get; set; }
        public bool Activa { get; set; }
    
        public virtual Aeropuerto Aeropuerto { get; set; }
        public virtual Aeropuerto Aeropuerto1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Frecuencia> Frecuencias { get; set; }
    }
}
