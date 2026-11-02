namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ReservaPasajeroVuelo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ReservaPasajeroVuelo()
        {
            this.CheckIns = new HashSet<CheckIn>();
        }
    
        public int AsignacionId { get; set; }
        public int ReservaId { get; set; }
        public int PasajeroId { get; set; }
        public int VueloId { get; set; }
        public byte NumeroAsiento { get; set; }
        public decimal ImporteTramo { get; set; }
        public string EstadoAsignacionCodigo { get; set; }
        public System.DateTime FechaAsignacion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CheckIn> CheckIns { get; set; }
        public virtual EstadoAsignacion EstadoAsignacion { get; set; }
        public virtual Pasajero Pasajero { get; set; }
        public virtual Reserva Reserva { get; set; }
        public virtual Vuelo Vuelo { get; set; }
    }
}
