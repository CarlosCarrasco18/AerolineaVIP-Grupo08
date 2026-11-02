namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CheckIn
    {
        public int CheckInId { get; set; }
        public int AsignacionId { get; set; }
        public System.DateTime FechaRegistro { get; set; }
        public string EstadoCheckInCodigo { get; set; }
    
        public virtual ReservaPasajeroVuelo ReservaPasajeroVuelo { get; set; }
        public virtual EstadoCheckIn EstadoCheckIn { get; set; }
    }
}
