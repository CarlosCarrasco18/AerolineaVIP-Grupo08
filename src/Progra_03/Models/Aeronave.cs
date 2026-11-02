namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Aeronave
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Aeronave()
        {
            this.Vueloes = new HashSet<Vuelo>();
        }
    
        public int AeronaveId { get; set; }
        public string Matricula { get; set; }
        public int ModeloAeronaveId { get; set; }
        public string EstadoAeronaveCodigo { get; set; }
    
        public virtual EstadoAeronave EstadoAeronave { get; set; }
        public virtual ModeloAeronave ModeloAeronave { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vuelo> Vueloes { get; set; }
    }
}
